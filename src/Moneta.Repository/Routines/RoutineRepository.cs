using Moneta.Application.Routines;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.Routines;

public class RoutineRepository(AppDbContext db) : IRoutineRepository
{
    public Task<List<Routine>> GetAllAsync(CancellationToken ct = default) =>
        db.Routines
            .Include(r => r.Module)
            .Include(r => r.Executions)
            .AsNoTracking()
            .OrderBy(r => r.Title)
            .ToListAsync(ct);

    public Task<Routine?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Routines
            .Include(r => r.Module)
            .Include(r => r.Executions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(Routine routine, CancellationToken ct = default)
    {
        db.Routines.Add(routine);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Routine routine, CancellationToken ct = default) =>
        // `routine` always comes from this same (request-scoped) context's GetByIdAsync, so it's
        // already tracked — no explicit Update() needed.
        db.SaveChangesAsync(ct);

    public async Task RemoveAsync(Routine routine, CancellationToken ct = default)
    {
        db.Routines.Remove(routine);
        await db.SaveChangesAsync(ct);
    }
}
