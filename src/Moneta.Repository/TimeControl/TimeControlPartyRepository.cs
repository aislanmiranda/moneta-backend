using Moneta.Application.TimeControl;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.TimeControl;

public class TimeControlPartyRepository(AppDbContext db) : ITimeControlPartyRepository
{
    public Task<List<TimeControlParty>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        db.TimeControlParties.Where(p => p.ModuleId == moduleId).AsNoTracking().ToListAsync(ct);

    public Task<TimeControlParty?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TimeControlParties.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(TimeControlParty party, CancellationToken ct = default)
    {
        db.TimeControlParties.Add(party);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(TimeControlParty party, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default) =>
        db.TimeControlParties.AnyAsync(p => p.ModuleId == moduleId && p.Id != exceptId && p.Name.ToLower() == name.ToLower(), ct);

    public Task<int> CountUsageAsync(Guid id, CancellationToken ct = default) =>
        db.TimeControlItems.Where(i => i.TimeControlPartyId == id).CountAsync(ct);

    public Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default) =>
        db.TimeControlItems.Where(i => i.ModuleId == moduleId).GroupBy(i => i.TimeControlPartyId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public async Task RemoveAsync(TimeControlParty party, CancellationToken ct = default)
    {
        db.TimeControlParties.Remove(party);
        await db.SaveChangesAsync(ct);
    }
}
