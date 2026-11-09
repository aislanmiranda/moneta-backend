using Moneta.Application.Routines;
using Moneta.Domain.Enums;

namespace Moneta.Application.TimeControl;

public record TimeControlItemDto(
    Guid Id,
    Guid ModuleId,
    string Title,
    Guid TimeControlTypeId,
    string TypeName,
    string TypeIcon,
    Guid TimeControlPartyId,
    string TimeControlPartyName,
    string? Dosage,
    TimeControlItemStatus Status,
    string[]? DaysOfWeek,
    int[] Hours,
    int Minute,
    string CronExpression,
    bool IsAdvanced
);

public record CreateTimeControlItemDto(
    Guid ModuleId,
    string Title,
    Guid TimeControlTypeId,
    Guid TimeControlPartyId,
    string? Dosage,
    ScheduleInputDto Schedule
);

public record UpdateTimeControlItemDto(
    string Title,
    Guid TimeControlTypeId,
    Guid TimeControlPartyId,
    string? Dosage,
    ScheduleInputDto Schedule
);

public record UpdateTimeControlItemStatusDto(TimeControlItemStatus Status);
