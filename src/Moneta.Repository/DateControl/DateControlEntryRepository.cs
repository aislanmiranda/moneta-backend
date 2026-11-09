using Moneta.Application.DateControl;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.DateControl;

public class DateControlEntryRepository(AppDbContext db) : IDateControlEntryRepository
{
    private static IQueryable<DateControlEntry> WithRelated(IQueryable<DateControlEntry> query) =>
        query.Include(e => e.DateControlSource).ThenInclude(s => s.DateControlCategory).Include(e => e.DateControlParty);

    public Task<List<DateControlEntry>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        WithRelated(db.DateControlEntries).Where(e => e.ModuleId == moduleId).AsNoTracking().ToListAsync(ct);

    public Task<DateControlEntry?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        WithRelated(db.DateControlEntries).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(DateControlEntry entry, CancellationToken ct = default)
    {
        db.DateControlEntries.Add(entry);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<DateControlEntry> entries, CancellationToken ct = default)
    {
        db.DateControlEntries.AddRange(entries);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DateControlEntry entry, CancellationToken ct = default)
    {
        db.DateControlEntries.Update(entry);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(DateControlEntry entry, CancellationToken ct = default)
    {
        db.DateControlEntries.Remove(entry);
        await db.SaveChangesAsync(ct);
    }

    public Task<DateControlEntry?> FindOccurrenceAsync(
        Guid moduleId, Guid sourceId, Guid partyId, DateOnly dueDate, CancellationToken ct = default) =>
        db.DateControlEntries.FirstOrDefaultAsync(e =>
            e.ModuleId == moduleId &&
            e.DateControlSourceId == sourceId &&
            e.DateControlPartyId == partyId &&
            e.DueDate == dueDate, ct);

    public Task<bool> ExistsOccurrenceAsync(
        Guid moduleId, Guid sourceId, Guid partyId, DateOnly dueDate, CancellationToken ct = default) =>
        db.DateControlEntries.AnyAsync(e =>
            e.ModuleId == moduleId &&
            e.DateControlSourceId == sourceId &&
            e.DateControlPartyId == partyId &&
            e.DueDate == dueDate, ct);

    public Task<Dictionary<Guid, int>> GetAttachmentCountsAsync(
        IReadOnlyCollection<Guid> entryIds, CancellationToken ct = default) =>
        db.DateControlEntryAttachments
            .Where(a => entryIds.Contains(a.DateControlEntryId))
            .GroupBy(a => a.DateControlEntryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public Task<List<DateControlEntryAttachmentDto>> GetAttachmentsAsync(Guid entryId, CancellationToken ct = default) =>
        db.DateControlEntryAttachments
            .Where(a => a.DateControlEntryId == entryId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new DateControlEntryAttachmentDto(a.Id, a.FileName, a.ContentType, a.SizeBytes, a.CreatedAt))
            .ToListAsync(ct);

    public Task<DateControlEntryAttachment?> GetAttachmentAsync(
        Guid entryId, Guid attachmentId, CancellationToken ct = default) =>
        db.DateControlEntryAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.DateControlEntryId == entryId, ct);

    public async Task AddAttachmentAsync(DateControlEntryAttachment attachment, CancellationToken ct = default)
    {
        db.DateControlEntryAttachments.Add(attachment);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> RemoveAttachmentAsync(Guid entryId, Guid attachmentId, CancellationToken ct = default) =>
        await db.DateControlEntryAttachments
            .Where(a => a.Id == attachmentId && a.DateControlEntryId == entryId)
            .ExecuteDeleteAsync(ct) > 0;
}
