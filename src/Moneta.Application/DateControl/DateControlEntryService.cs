using Moneta.Domain.Entities;
using Moneta.Domain.Enums;
using Moneta.Domain.Exceptions;

namespace Moneta.Application.DateControl;

public class DateControlEntryService(
    IDateControlEntryRepository repository,
    IDateControlSourceRepository sourceRepository,
    IDateControlPartyRepository partyRepository
) : IDateControlEntryService
{
    public const long MaxAttachmentBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedAttachmentTypes =
        ["application/pdf", "image/jpeg", "image/png", "image/webp", "image/heic", "image/heif"];

    public async Task<List<DateControlEntryDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default)
    {
        var entries = await repository.GetAllAsync(moduleId, ct);
        var counts = await repository.GetAttachmentCountsAsync(entries.Select(e => e.Id).ToList(), ct);
        return entries.Select(e => ToDto(e, counts.GetValueOrDefault(e.Id))).ToList();
    }

    public async Task<DateControlEntryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await repository.GetByIdAsync(id, ct);
        return entry is null ? null : await ToDtoWithCountAsync(entry, ct);
    }

    public async Task<DateControlEntryDto> CreateAsync(CreateDateControlEntryDto input, CancellationToken ct = default)
    {
        var source = await sourceRepository.GetByIdAsync(input.DateControlSourceId, ct)
            ?? throw new NotFoundException("Conta financeira não encontrada.");

        var party = await partyRepository.GetByIdAsync(input.DateControlPartyId, ct)
            ?? throw new NotFoundException("Centro de custo não encontrado.");

        // Installments and forecast recurrences are pre-generated now (every occurrence becomes
        // its own row); an indeterminate recurrence has no end, so only the first occurrence is
        // created here and each next one is created when the current one is paid (see
        // UpdateStatusAsync). Guarded rather than asserted: an invalid/missing count falls back
        // to a single entry, and EnsureValid() below raises the real validation error for it.
        var isForecast = input.RecurrenceType is
            DateControlRecurrenceType.ForecastDays or DateControlRecurrenceType.ForecastMonths;
        var occurrenceCount = input.InstallmentsCount is > 1
            ? input.InstallmentsCount.Value
            : isForecast && input.RecurrenceCount is > 0 and <= DateControlEntry.MaxForecastCount
                ? input.RecurrenceCount.Value
                : 1;

        var entries = new List<DateControlEntry>(occurrenceCount);
        for (var i = 0; i < occurrenceCount; i++)
        {
            var dueDate = input.RecurrenceType == DateControlRecurrenceType.ForecastDays
                ? input.DueDate.AddDays(i)
                : input.DueDate.AddMonths(i);
            var entry = new DateControlEntry
            {
                Id = Guid.NewGuid(),
                ModuleId = input.ModuleId,
                DateControlSourceId = source.Id,
                DateControlSource = source,
                DateControlPartyId = party.Id,
                DateControlParty = party,
                Amount = input.Amount,
                DueDate = dueDate,
                InstallmentsCount = input.InstallmentsCount,
                RecurrenceType = input.RecurrenceType,
                RecurrenceCount = input.RecurrenceCount,
                OccurrenceNumber = occurrenceCount > 1 || input.RecurrenceType == DateControlRecurrenceType.Indeterminate
                    ? i + 1
                    : null,
                Scope = input.Scope,
                Notes = NormalizeNotes(input.Notes),
                // Matches DateControlReminderJob's own overdue sweep (Pending + DueDate < today →
                // Overdue) so a backdated entry shows the right status immediately instead of
                // waiting for the next worker tick.
                Status = ResolveStatus(dueDate, DateControlEntryStatus.Pending),
            };

            entry.EnsureValid();
            entries.Add(entry);
        }

        await repository.AddRangeAsync(entries, ct);
        return ToDto(entries[0], 0);
    }

    public async Task<DateControlEntryDto?> UpdateAsync(
        Guid id, UpdateDateControlEntryDto input, CancellationToken ct = default)
    {
        var entry = await repository.GetByIdAsync(id, ct);
        if (entry is null) return null;

        var party = await partyRepository.GetByIdAsync(input.DateControlPartyId, ct)
            ?? throw new NotFoundException("Centro de custo não encontrado.");

        entry.DateControlPartyId = party.Id;
        entry.DateControlParty = party;
        entry.Amount = input.Amount;
        entry.DueDate = input.DueDate;
        entry.Scope = input.Scope;
        entry.Notes = NormalizeNotes(input.Notes);
        // Re-derive Pending/Overdue from the (possibly just-edited) due date — a Paid entry stays
        // Paid regardless of date edits, only the worker's own sweep or UpdateStatusAsync changes that.
        entry.Status = ResolveStatus(input.DueDate, entry.Status);

        entry.EnsureValid();

        await repository.UpdateAsync(entry, ct);
        return await ToDtoWithCountAsync(entry, ct);
    }

    public async Task<DateControlEntryDto?> UpdateStatusAsync(
        Guid id, DateControlEntryStatus status, CancellationToken ct = default)
    {
        var entry = await repository.GetByIdAsync(id, ct);
        if (entry is null) return null;

        var wasPaid = entry.Status == DateControlEntryStatus.Paid;
        var becamePaid = status == DateControlEntryStatus.Paid && !wasPaid;
        var becameUnpaid = status != DateControlEntryStatus.Paid && wasPaid;

        // Unpaid is Pending or Overdue depending on the due date — never trust the caller to pick,
        // so "desmarcar pagamento" of a past-due entry lands back on Overdue.
        entry.Status = status == DateControlEntryStatus.Paid
            ? DateControlEntryStatus.Paid
            : ResolveStatus(entry.DueDate, DateControlEntryStatus.Pending);

        await repository.UpdateAsync(entry, ct);

        if (entry.RecurrenceType == DateControlRecurrenceType.Indeterminate)
        {
            if (becamePaid) await CreateNextIndeterminateOccurrenceAsync(entry, ct);
            if (becameUnpaid) await RemoveUntouchedNextOccurrenceAsync(entry, ct);
        }

        return await ToDtoWithCountAsync(entry, ct);
    }

    /// <summary>Undo for a payment marked by mistake: drops the occurrence that payment generated,
    /// but only while it's still exactly as generated (not paid, no notes, no attachments) — if
    /// the user already worked on it, it stays.</summary>
    private async Task RemoveUntouchedNextOccurrenceAsync(DateControlEntry unpaid, CancellationToken ct)
    {
        var next = await repository.FindOccurrenceAsync(
            unpaid.ModuleId, unpaid.DateControlSourceId, unpaid.DateControlPartyId, unpaid.DueDate.AddMonths(1), ct);

        if (next is null ||
            next.RecurrenceType != DateControlRecurrenceType.Indeterminate ||
            next.Status == DateControlEntryStatus.Paid ||
            next.Notes is not null)
        {
            return;
        }

        var attachments = await repository.GetAttachmentCountsAsync([next.Id], ct);
        if (attachments.GetValueOrDefault(next.Id) > 0) return;

        await repository.RemoveAsync(next, ct);
    }

    /// <summary>Paying an indeterminate entry rolls the series forward by one month. Skipped when
    /// that occurrence already exists (paid → unpaid → paid again, or created by hand), so the
    /// series never gets duplicates. Notes and attachments aren't copied — they're specific to
    /// each month's bill.</summary>
    private async Task CreateNextIndeterminateOccurrenceAsync(DateControlEntry paid, CancellationToken ct)
    {
        var nextDueDate = paid.DueDate.AddMonths(1);

        if (await repository.ExistsOccurrenceAsync(
                paid.ModuleId, paid.DateControlSourceId, paid.DateControlPartyId, nextDueDate, ct))
        {
            return;
        }

        var next = new DateControlEntry
        {
            Id = Guid.NewGuid(),
            ModuleId = paid.ModuleId,
            DateControlSourceId = paid.DateControlSourceId,
            DateControlPartyId = paid.DateControlPartyId,
            Amount = paid.Amount,
            DueDate = nextDueDate,
            RecurrenceType = DateControlRecurrenceType.Indeterminate,
            OccurrenceNumber = (paid.OccurrenceNumber ?? 1) + 1,
            Scope = paid.Scope,
            Status = ResolveStatus(nextDueDate, DateControlEntryStatus.Pending),
        };

        next.EnsureValid();
        await repository.AddAsync(next, ct);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await repository.GetByIdAsync(id, ct);
        if (entry is null) return false;

        await repository.RemoveAsync(entry, ct);
        return true;
    }

    /// <summary>Paid is a terminal, user-driven state — a date edit never moves an entry out of
    /// it. Otherwise mirrors DateControlReminderJob's own sweep: due date in the past → Overdue,
    /// future or today → Pending. Local date, not UTC, same convention used throughout
    /// schedule/ ("today" means the calendar day on the user's own machine).</summary>
    private static DateControlEntryStatus ResolveStatus(DateOnly dueDate, DateControlEntryStatus currentStatus)
    {
        if (currentStatus == DateControlEntryStatus.Paid) return DateControlEntryStatus.Paid;

        var today = DateOnly.FromDateTime(DateTime.Now);
        return dueDate < today ? DateControlEntryStatus.Overdue : DateControlEntryStatus.Pending;
    }

    public async Task<List<DateControlEntryAttachmentDto>?> GetAttachmentsAsync(
        Guid entryId, CancellationToken ct = default)
    {
        if (await repository.GetByIdAsync(entryId, ct) is null) return null;
        return await repository.GetAttachmentsAsync(entryId, ct);
    }

    public async Task<DateControlEntryAttachmentDto?> AddAttachmentAsync(
        Guid entryId, UploadAttachmentInput input, CancellationToken ct = default)
    {
        if (await repository.GetByIdAsync(entryId, ct) is null) return null;

        if (input.Content.Length == 0)
            throw new DomainValidationException("O arquivo está vazio.");

        if (input.Content.Length > MaxAttachmentBytes)
            throw new DomainValidationException("O arquivo pode ter no máximo 10 MB.");

        var contentType = input.ContentType.ToLowerInvariant();
        if (!AllowedAttachmentTypes.Contains(contentType))
            throw new DomainValidationException("Envie um PDF ou uma imagem (JPG, PNG, WEBP ou HEIC).");

        var attachment = new DateControlEntryAttachment
        {
            Id = Guid.NewGuid(),
            DateControlEntryId = entryId,
            FileName = Path.GetFileName(input.FileName) is { Length: > 0 } name ? name[..Math.Min(name.Length, 255)] : "comprovante",
            ContentType = contentType,
            SizeBytes = input.Content.Length,
            Content = input.Content,
        };

        await repository.AddAttachmentAsync(attachment, ct);
        return new DateControlEntryAttachmentDto(
            attachment.Id, attachment.FileName, attachment.ContentType, attachment.SizeBytes, attachment.CreatedAt);
    }

    public async Task<AttachmentFile?> GetAttachmentFileAsync(
        Guid entryId, Guid attachmentId, CancellationToken ct = default)
    {
        var attachment = await repository.GetAttachmentAsync(entryId, attachmentId, ct);
        return attachment is null ? null : new AttachmentFile(attachment.FileName, attachment.ContentType, attachment.Content);
    }

    public Task<bool> RemoveAttachmentAsync(Guid entryId, Guid attachmentId, CancellationToken ct = default) =>
        repository.RemoveAttachmentAsync(entryId, attachmentId, ct);

    private static string? NormalizeNotes(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private async Task<DateControlEntryDto> ToDtoWithCountAsync(DateControlEntry entry, CancellationToken ct)
    {
        var counts = await repository.GetAttachmentCountsAsync([entry.Id], ct);
        return ToDto(entry, counts.GetValueOrDefault(entry.Id));
    }

    private static DateControlEntryDto ToDto(DateControlEntry e, int attachmentsCount) => new(
        e.Id,
        e.ModuleId,
        e.DateControlSourceId,
        e.DateControlSource.Description,
        e.DateControlSource.DateControlCategory.Name,
        e.DateControlSource.DateControlCategory.Icon,
        e.DateControlPartyId,
        e.DateControlParty.Name,
        e.Amount,
        e.DueDate,
        e.InstallmentsCount,
        e.RecurrenceType,
        e.RecurrenceCount,
        e.Status,
        e.OccurrenceNumber,
        e.Scope,
        e.Notes,
        attachmentsCount
    );
}
