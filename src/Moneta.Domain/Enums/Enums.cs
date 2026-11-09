namespace Moneta.Domain.Enums;

/// <summary>Active routines get a Quartz trigger; Paused ones keep their config but don't fire.</summary>
public enum RoutineStatus
{
    Active,
    Paused,
}

/// <summary>Which recurring handler in schedule/ runs this routine — one per ModulePattern, shared
/// by every Module of that pattern (Routine.ModuleId says which instance). Add a value here
/// whenever a new pattern gets a handler — ScheduleSyncJob maps it to a JobKey (see
/// schedule/README.md). Ordinal values preserved from the former AgentTaskType so rows migrated
/// from AgentTasks keep their meaning.</summary>
public enum RoutineType
{
    DateControlReminder,
    TimeControlReminder,
    DateTimeControlReminder,
}

/// <summary>Who a party (DateControlParty/TimeControlParty) represents — presets for the common
/// household cases, plus Other for custom names. Shared across patterns rather than duplicated,
/// since the relationship vocabulary is identical; the parties themselves stay separate tables
/// per pattern/module.</summary>
public enum PartyRelationship
{
    Self,
    Father,
    Mother,
    Spouse,
    Child,
    Other,
}

public enum RoutineExecutionStatus
{
    Running,
    Succeeded,
    Failed,
}
