using Moneta.Application.DateControl;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.DateControl;

public class DateControlSourceRepository(AppDbContext db) : IDateControlSourceRepository
{
    public Task<List<DateControlSource>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        db.DateControlSources.Include(a => a.Entries).Include(a => a.DateControlCategory).Where(a => a.ModuleId == moduleId).AsNoTracking().ToListAsync(ct);

    public Task<DateControlSource?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.DateControlSources.Include(a => a.Entries).Include(a => a.DateControlCategory).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(DateControlSource source, CancellationToken ct = default)
    {
        db.DateControlSources.Add(source);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(DateControlSource source, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task RemoveAsync(DateControlSource source, CancellationToken ct = default)
    {
        db.DateControlSources.Remove(source);
        await db.SaveChangesAsync(ct);
    }
}
