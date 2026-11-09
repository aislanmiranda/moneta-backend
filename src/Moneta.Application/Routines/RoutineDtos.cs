using Moneta.Domain.Enums;

namespace Moneta.Application.Routines;

/// <summary>
/// "Modo simples": DaysOfWeek + Hours + Minute, composed into a cron via CronBuilder.
/// "Modo avançado": RawCron non-empty overrides everything else — stored as-is, validated via
/// Quartz.CronExpression.TryParse.
/// </summary>
public record ScheduleInputDto(string[]? DaysOfWeek, int[] Hours, int Minute, string? RawCron);

/// <summary>
/// Escalation rules for a DateControl-pattern Routine — only meaningful when the routine's
/// resolved Type is RoutineType.DateControlReminder (null for every other pattern). Each field
/// is an independent point/condition the worker checks on every tick (see
/// schedule/Jobs/DateControlReminderJob.cs): remind BeforeDueDays days before due date at
/// BeforeDueTime; remind once OnDueDate; and if AfterDueRepeatMinutes is set, keep reminding
/// every N minutes after the due date passes until the entry is marked Paid.
/// </summary>
public record ReminderRulesDto(
    int? BeforeDueDays,
    string? BeforeDueTime,
    bool OnDueDate,
    int? AfterDueRepeatMinutes
);

public record RoutineDto(
    Guid Id,
    string Title,
    Guid ModuleId,
    string ModuleName,
    RoutineType Type,
    string[]? DaysOfWeek,
    int[] Hours,
    int Minute,
    string CronExpression,
    bool IsAdvanced,
    DateTimeOffset? NextRunAt,
    DateTimeOffset? LastExecutedAt,
    DateTimeOffset? TriggerRequestedAt,
    ReminderRulesDto? ReminderRules,
    RoutineStatus Status
);

/// <summary>No Type here — it's derived from the chosen Module's Pattern (see
/// RoutineService.CreateAsync), so creating a routine means picking WHICH module to monitor,
/// not a freestanding type disconnected from any data.</summary>
public record CreateRoutineDto(
    string Title,
    Guid ModuleId,
    ScheduleInputDto Schedule,
    ReminderRulesDto? ReminderRules
);

public record UpdateRoutineDto(
    string Title,
    ScheduleInputDto Schedule,
    ReminderRulesDto? ReminderRules
);

public record UpdateRoutineStatusDto(RoutineStatus Status);
