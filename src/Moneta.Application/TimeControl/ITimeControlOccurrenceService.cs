using Moneta.Domain.Enums;

namespace Moneta.Application.TimeControl;

public interface ITimeControlOccurrenceService
{
    /// <summary>Null from/to defaults to today (local time — this runs on the user's own machine).</summary>
    Task<List<TimeControlOccurrenceDto>> GetAllAsync(Guid moduleId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<TimeControlOccurrenceDto?> UpdateStatusAsync(Guid id, TimeControlOccurrenceStatus status, CancellationToken ct = default);
}
