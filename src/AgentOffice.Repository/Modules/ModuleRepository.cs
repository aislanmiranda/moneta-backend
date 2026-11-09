using AgentOffice.Application.Modules;
using AgentOffice.Domain.Entities;
using AgentOffice.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentOffice.Repository.Modules;

public class ModuleRepository(AppDbContext db) : IModuleRepository
{
    public Task<List<Module>> GetAllAsync(CancellationToken ct = default) =>
        db.Modules.AsNoTracking().OrderBy(m => m.Name).ToListAsync(ct);

    public Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Modules.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddAsync(Module module, IEnumerable<object> starterRecords, CancellationToken ct = default)
    {
        db.Modules.Add(module);
        db.AddRange(starterRecords);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        db.Modules.AnyAsync(m => m.Slug == slug, ct);
}
