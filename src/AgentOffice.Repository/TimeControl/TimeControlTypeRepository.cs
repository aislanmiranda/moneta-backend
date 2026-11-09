using AgentOffice.Application.TimeControl;
using AgentOffice.Domain.Entities;
using AgentOffice.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentOffice.Repository.TimeControl;

public class TimeControlTypeRepository(AppDbContext db) : ITimeControlTypeRepository
{
    public Task<List<TimeControlType>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        db.TimeControlTypes.Where(x => x.ModuleId == moduleId).OrderBy(x => x.Name).AsNoTracking().ToListAsync(ct);

    public Task<TimeControlType?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TimeControlTypes.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TimeControlType entity, CancellationToken ct = default)
    {
        db.TimeControlTypes.Add(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(TimeControlType entity, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task RemoveAsync(TimeControlType entity, CancellationToken ct = default)
    {
        db.TimeControlTypes.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default) =>
        db.TimeControlTypes.AnyAsync(x => x.ModuleId == moduleId && x.Id != exceptId && x.Name.ToLower() == name.ToLower(), ct);

    public Task<int> CountUsageAsync(Guid id, CancellationToken ct = default) =>
        db.TimeControlItems.Where(i => i.TimeControlTypeId == id).CountAsync(ct);

    public Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default) =>
        db.TimeControlItems.Where(i => i.ModuleId == moduleId).GroupBy(i => i.TimeControlTypeId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
}
