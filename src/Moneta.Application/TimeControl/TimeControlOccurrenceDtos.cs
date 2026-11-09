using Moneta.Domain.Enums;

namespace Moneta.Application.TimeControl;

public record TimeControlOccurrenceDto(
    Guid Id,
    Guid TimeControlItemId,
    string TimeControlItemTitle,
    string TypeName,
    string TypeIcon,
    string TimeControlPartyName,
    DateTimeOffset ScheduledAt,
    TimeControlOccurrenceStatus Status,
    DateTimeOffset? DoneAt
);

public record UpdateTimeControlOccurrenceStatusDto(TimeControlOccurrenceStatus Status);
