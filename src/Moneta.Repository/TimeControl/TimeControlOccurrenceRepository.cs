using Moneta.Application.TimeControl;
using Moneta.Domain;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.TimeControl;

public class TimeControlOccurrenceRepository(AppDbContext db) : ITimeControlOccurrenceRepository
{
    public Task<List<TimeControlOccurrence>> GetAllAsync(Guid moduleId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        // The requested days are local calendar days (app timezone), compared as UTC instants:
        // Npgsql only accepts Offset=0 for timestamptz.
        var fromUtc = AppTime.ToUtc(from, TimeOnly.MinValue);
        var toUtc = AppTime.ToUtc(to.AddDays(1), TimeOnly.MinValue).AddTicks(-1);

        return db.TimeControlOccurrences
            .Include(l => l.TimeControlItem)
            .ThenInclude(i => i.TimeControlParty)
            .Include(l => l.TimeControlItem)
            .ThenInclude(i => i.TimeControlType)
            .AsNoTracking()
            .Where(l => l.TimeControlItem.ModuleId == moduleId && l.ScheduledAt >= fromUtc && l.ScheduledAt <= toUtc)
            .OrderBy(l => l.ScheduledAt)
            .ToListAsync(ct);
    }

    public Task<TimeControlOccurrence?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TimeControlOccurrences
            .Include(l => l.TimeControlItem)
            .ThenInclude(i => i.TimeControlParty)
            .Include(l => l.TimeControlItem)
            .ThenInclude(i => i.TimeControlType)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task UpdateAsync(TimeControlOccurrence occurrence, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
