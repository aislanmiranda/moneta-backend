using Moneta.Application.DateTimeControl;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.DateTimeControl;

public class DateTimeControlEntryRepository(AppDbContext db) : IDateTimeControlEntryRepository
{
    public Task<List<DateTimeControlEntry>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        db.DateTimeControlEntries.AsNoTracking()
            .Where(e => e.ModuleId == moduleId)
            .OrderBy(e => e.ScheduledAt)
            .ToListAsync(ct);

    public Task<DateTimeControlEntry?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.DateTimeControlEntries.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(DateTimeControlEntry entry, CancellationToken ct = default)
    {
        db.DateTimeControlEntries.Add(entry);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DateTimeControlEntry entry, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(DateTimeControlEntry entry, CancellationToken ct = default)
    {
        db.DateTimeControlEntries.Remove(entry);
        await db.SaveChangesAsync(ct);
    }
}
