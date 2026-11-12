using Moneta.Application.DateControl;
using Moneta.Domain.Entities;
using Moneta.Domain.Enums;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.DateControl;

public class DateControlEntryRepository(AppDbContext db) : IDateControlEntryRepository
{
    private static IQueryable<DateControlEntry> WithRelated(IQueryable<DateControlEntry> query) =>
        query.Include(e => e.DateControlSource).ThenInclude(s => s.DateControlCategory).Include(e => e.DateControlParty);

    public Task<List<DateControlEntry>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        WithRelated(db.DateControlEntries).Where(e => e.ModuleId == moduleId).AsNoTracking().ToListAsync(ct);

    public async Task<List<DashboardAggregateRow>> GetMonthlyAggregateAsync(
        Guid moduleId, int year, DateOnly today, CancellationToken ct = default)
    {
        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        var grouped = await db.DateControlEntries
            .AsNoTracking()
            .Where(e => e.ModuleId == moduleId && e.DueDate >= start && e.DueDate <= end)
            .GroupBy(e => new
            {
                e.DueDate.Month,
                Bucket = e.Status == DateControlEntryStatus.Paid ? 0 : e.DueDate < today ? 1 : 2,
            })
            .Select(g => new { g.Key.Month, g.Key.Bucket, Total = g.Sum(x => x.Amount) })
            .ToListAsync(ct);
        return grouped.Select(g => new DashboardAggregateRow(g.Month, (DashboardBucket)g.Bucket, g.Total)).ToList();
    }

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
