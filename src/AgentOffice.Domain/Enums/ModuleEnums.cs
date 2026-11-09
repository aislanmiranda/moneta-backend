namespace AgentOffice.Domain.Enums;

/// <summary>Which recurring-tracking shape a Module follows — drives which generic entity set
/// (DateControl*/TimeControl*/DateTimeControl*) holds its records, which form fields the UI
/// shows, and which RoutineType/Job handles its reminders.</summary>
public enum ModulePattern
{
    /// <summary>Tracked by a due date, no specific time (e.g. Financeiro).</summary>
    DateControl,

    /// <summary>Tracked by a recurring time of day (e.g. Vida Saudável).</summary>
    TimeControl,

    /// <summary>Tracked by a specific date AND time (e.g. Agenda).</summary>
    DateTimeControl,
}

public enum DateTimeControlEntryStatus
{
    Scheduled,
    Done,
    Cancelled,
    Missed,
}
