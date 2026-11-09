using Moneta.Application.TimeControl;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.TimeControl;

public class TimeControlOccurrenceRepository(AppDbContext db) : ITimeControlOccurrenceRepository
{
    public Task<List<TimeControlOccurrence>> GetAllAsync(Guid moduleId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        // DateTimeOffset(DateTime) with an Unspecified Kind (what DateOnly.ToDateTime returns)
        // resolves against the local system timezone — same "local, not UTC" convention used
        // throughout schedule/ for "what day is it" (see FinanceReminderJob). Converted to UTC
        // before use as a query parameter: Npgsql only accepts Offset=0 for timestamptz.
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue)).ToUniversalTime();
        var toUtc = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue)).ToUniversalTime();

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
