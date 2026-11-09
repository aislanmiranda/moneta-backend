using Moneta.Domain.Enums;

namespace Moneta.Domain.Entities;

/// <summary>
/// A user-created recurring job: "check this Module on this cron and notify me". The schedule
/// worker (see schedule/Jobs/ScheduleSyncJob.cs) keeps one Quartz trigger per Active routine,
/// keyed by its id. Owns its schedule directly (flattened, same shape as TimeControlItem) since
/// it's always 1:1.
/// </summary>
public class Routine
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    /// <summary>Which Module instance this routine monitors — the handler (see Type) reads only
    /// this module's records, not every module sharing the same pattern.</summary>
    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public RoutineType Type { get; set; }

    /// <summary>Free-form JSON payload with type-specific parameters (e.g. reminder rules for
    /// DateControlReminder). Shape depends on Type — see schedule/Jobs for what each one reads.</summary>
    public string? Payload { get; set; }

    public RoutineStatus Status { get; set; } = RoutineStatus.Active;

    // Same shape as TimeControlItem — composed/validated via CronBuilder.
    public required string CronExpression { get; set; }
    public required string TimeZone { get; set; }

    /// <summary>CSV of 3-letter day codes (e.g. "MON,WED,FRI"); null/empty = every day.</summary>
    public string? DaysOfWeek { get; set; }

    /// <summary>CSV of hours 0-23 (e.g. "8,16,23"). Null means CronExpression was typed
    /// directly by the user ("modo avançado") — DaysOfWeek/Hours/Minute aren't meaningful.</summary>
    public string? Hours { get; set; }

    public int Minute { get; set; }

    /// <summary>Written back by ScheduleSyncJob from the live Quartz trigger, so the UI can show
    /// "próxima execução".</summary>
    public DateTimeOffset? NextRunAt { get; set; }

    /// <summary>Set by "Executar agora"; cleared by ScheduleSyncJob once it fires this routine
    /// out-of-band. Never read by anything but the worker.</summary>
    public DateTimeOffset? TriggerRequestedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<RoutineExecution> Executions { get; set; } = [];
}
