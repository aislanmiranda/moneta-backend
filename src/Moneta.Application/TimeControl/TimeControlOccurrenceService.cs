using Moneta.Domain.Entities;
using Moneta.Domain.Enums;

namespace Moneta.Application.TimeControl;

public class TimeControlOccurrenceService(ITimeControlOccurrenceRepository repository) : ITimeControlOccurrenceService
{
    public async Task<List<TimeControlOccurrenceDto>> GetAllAsync(
        Guid moduleId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var occurrences = await repository.GetAllAsync(moduleId, from ?? today, to ?? today, ct);
        return occurrences.Select(ToDto).ToList();
    }

    public async Task<TimeControlOccurrenceDto?> UpdateStatusAsync(
        Guid id, TimeControlOccurrenceStatus status, CancellationToken ct = default)
    {
        var occurrence = await repository.GetByIdAsync(id, ct);
        if (occurrence is null) return null;

        occurrence.Status = status;
        occurrence.DoneAt = status == TimeControlOccurrenceStatus.Done ? DateTimeOffset.UtcNow : null;

        await repository.UpdateAsync(occurrence, ct);
        return ToDto(occurrence);
    }

    private static TimeControlOccurrenceDto ToDto(TimeControlOccurrence o) => new(
        o.Id,
        o.TimeControlItemId,
        o.TimeControlItem.Title,
        o.TimeControlItem.TimeControlType.Name,
        o.TimeControlItem.TimeControlType.Icon,
        o.TimeControlItem.TimeControlParty.Name,
        o.ScheduledAt,
        o.Status,
        o.DoneAt
    );
}
