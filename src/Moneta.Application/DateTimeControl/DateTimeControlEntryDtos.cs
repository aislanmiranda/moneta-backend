using Moneta.Domain.Enums;

namespace Moneta.Application.DateTimeControl;

public record DateTimeControlEntryDto(
    Guid Id,
    Guid ModuleId,
    string Title,
    DateTimeOffset ScheduledAt,
    int? DurationMinutes,
    string? Notes,
    DateTimeControlEntryStatus Status
);

public record CreateDateTimeControlEntryDto(
    Guid ModuleId,
    string Title,
    DateTimeOffset ScheduledAt,
    int? DurationMinutes,
    string? Notes
);

public record UpdateDateTimeControlEntryDto(
    string Title,
    DateTimeOffset ScheduledAt,
    int? DurationMinutes,
    string? Notes
);

public record UpdateDateTimeControlEntryStatusDto(DateTimeControlEntryStatus Status);
