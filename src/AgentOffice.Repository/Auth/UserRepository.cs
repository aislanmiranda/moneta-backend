using AgentOffice.Application.Auth;
using AgentOffice.Domain.Entities;
using AgentOffice.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentOffice.Repository.Auth;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByLoginAsync(string login, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Login == login, ct);

    public Task<bool> AnyAsync(CancellationToken ct = default) => db.Users.AnyAsync(ct);

    public Task<bool> HasUnownedModulesAsync(CancellationToken ct = default) =>
        db.Modules.IgnoreQueryFilters().AnyAsync(m => m.UserId == null, ct);

    public async Task<List<(Module Module, bool HasParties)>> GetUnownedModulesAsync(CancellationToken ct = default)
    {
        var rows = await db.Modules.IgnoreQueryFilters()
            .Where(m => m.UserId == null)
            .Select(m => new
            {
                Module = m,
                HasParties =
                    db.DateControlParties.IgnoreQueryFilters().Any(p => p.ModuleId == m.Id) ||
                    db.TimeControlParties.IgnoreQueryFilters().Any(p => p.ModuleId == m.Id),
            })
            .AsNoTracking()
            .ToListAsync(ct);
        return rows.Select(r => (r.Module, r.HasParties)).ToList();
    }

    public Task UpdateAsync(User user, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task AddWithModulesAsync(User user, bool claimUnownedModules, IReadOnlyList<Module> starterModules,
        IReadOnlyList<object> starterRecords, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        db.Users.Add(user);
        db.Modules.AddRange(starterModules);
        db.AddRange(starterRecords);
        await db.SaveChangesAsync(ct);

        if (claimUnownedModules)
        {
            // IgnoreQueryFilters: the caller isn't signed in yet, and unowned rows are invisible
            // to everyone by design.
            await db.Modules.IgnoreQueryFilters()
                .Where(m => m.UserId == null)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.UserId, user.Id), ct);
        }

        await transaction.CommitAsync(ct);
    }
}
