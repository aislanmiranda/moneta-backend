using AgentOffice.Application.DateControl;
using AgentOffice.Domain.Entities;
using AgentOffice.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentOffice.Repository.DateControl;

public class DateControlCategoryRepository(AppDbContext db) : IDateControlCategoryRepository
{
    public Task<List<DateControlCategory>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        db.DateControlCategories.Where(x => x.ModuleId == moduleId).OrderBy(x => x.Name).AsNoTracking().ToListAsync(ct);

    public Task<DateControlCategory?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.DateControlCategories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(DateControlCategory entity, CancellationToken ct = default)
    {
        db.DateControlCategories.Add(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(DateControlCategory entity, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task RemoveAsync(DateControlCategory entity, CancellationToken ct = default)
    {
        db.DateControlCategories.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default) =>
        db.DateControlCategories.AnyAsync(x => x.ModuleId == moduleId && x.Id != exceptId && x.Name.ToLower() == name.ToLower(), ct);

    public Task<int> CountUsageAsync(Guid id, CancellationToken ct = default) =>
        db.DateControlSources.Where(s => s.DateControlCategoryId == id).CountAsync(ct);

    public Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default) =>
        db.DateControlSources.Where(s => s.ModuleId == moduleId).GroupBy(s => s.DateControlCategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
}
