using Moneta.Domain.Entities;

namespace Moneta.Application.DateControl;

public interface IDateControlEntryRepository
{
    /// <summary>Loaded with DateControlSource and DateControlParty included, for DTO projection.</summary>
    Task<List<DateControlEntry>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    /// <summary>Soma de Amount por mês e classificação (paga/vencida/pendente) no ano, agregada em SQL.</summary>
    Task<List<DashboardAggregateRow>> GetMonthlyAggregateAsync(
        Guid moduleId, int year, DateOnly today, CancellationToken ct = default);
    Task<DateControlEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DateControlEntry entry, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<DateControlEntry> entries, CancellationToken ct = default);
    Task UpdateAsync(DateControlEntry entry, CancellationToken ct = default);
    Task RemoveAsync(DateControlEntry entry, CancellationToken ct = default);

    /// <summary>The same bill's (module + source + party) entry on that date, tracked; null if none.</summary>
    Task<DateControlEntry?> FindOccurrenceAsync(
        Guid moduleId, Guid sourceId, Guid partyId, DateOnly dueDate, CancellationToken ct = default);

    /// <summary>Whether the same bill (module + source + party) already has an entry on that date.</summary>
    Task<bool> ExistsOccurrenceAsync(
        Guid moduleId, Guid sourceId, Guid partyId, DateOnly dueDate, CancellationToken ct = default);

    /// <summary>Attachment count per entry id — counted in SQL, never loads file contents.</summary>
    Task<Dictionary<Guid, int>> GetAttachmentCountsAsync(IReadOnlyCollection<Guid> entryIds, CancellationToken ct = default);

    /// <summary>Metadata only — Content is not loaded.</summary>
    Task<List<DateControlEntryAttachmentDto>> GetAttachmentsAsync(Guid entryId, CancellationToken ct = default);
    Task<DateControlEntryAttachment?> GetAttachmentAsync(Guid entryId, Guid attachmentId, CancellationToken ct = default);
    Task AddAttachmentAsync(DateControlEntryAttachment attachment, CancellationToken ct = default);
    Task<bool> RemoveAttachmentAsync(Guid entryId, Guid attachmentId, CancellationToken ct = default);
}
