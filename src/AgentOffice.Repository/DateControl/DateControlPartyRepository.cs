using AgentOffice.Application.DateControl;
using AgentOffice.Domain.Entities;
using AgentOffice.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentOffice.Repository.DateControl;

public class DateControlPartyRepository(AppDbContext db) : IDateControlPartyRepository
{
    public Task<List<DateControlParty>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        db.DateControlParties.Where(c => c.ModuleId == moduleId).AsNoTracking().ToListAsync(ct);

    public Task<DateControlParty?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.DateControlParties.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(DateControlParty party, CancellationToken ct = default)
    {
        db.DateControlParties.Add(party);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(DateControlParty party, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default) =>
        db.DateControlParties.AnyAsync(p => p.ModuleId == moduleId && p.Id != exceptId && p.Name.ToLower() == name.ToLower(), ct);

    public Task<int> CountUsageAsync(Guid id, CancellationToken ct = default) =>
        db.DateControlEntries.Where(e => e.DateControlPartyId == id).CountAsync(ct);

    public Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default) =>
        db.DateControlEntries.Where(e => e.ModuleId == moduleId).GroupBy(e => e.DateControlPartyId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public async Task RemoveAsync(DateControlParty party, CancellationToken ct = default)
    {
        db.DateControlParties.Remove(party);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> AnyAsync(CancellationToken ct = default) => db.DateControlParties.AnyAsync(ct);
}
