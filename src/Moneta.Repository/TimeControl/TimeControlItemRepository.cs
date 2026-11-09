using Moneta.Application.TimeControl;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.TimeControl;

public class TimeControlItemRepository(AppDbContext db) : ITimeControlItemRepository
{
    public Task<List<TimeControlItem>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        db.TimeControlItems.Include(i => i.TimeControlParty).Include(i => i.TimeControlType).Where(i => i.ModuleId == moduleId).AsNoTracking().ToListAsync(ct);

    public Task<TimeControlItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TimeControlItems.Include(i => i.TimeControlParty).Include(i => i.TimeControlType).FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task AddAsync(TimeControlItem item, CancellationToken ct = default)
    {
        db.TimeControlItems.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TimeControlItem item, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(TimeControlItem item, CancellationToken ct = default)
    {
        db.TimeControlItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}
