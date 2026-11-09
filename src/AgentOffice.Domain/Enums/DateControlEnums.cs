namespace AgentOffice.Domain.Enums;

/// <summary>
/// Whether an entry repeats, and how its occurrences are created.
/// Distinct from <see cref="DateControlEntry.InstallmentsCount"/>: installments split a single
/// purchase into N payments, recurrence repeats the same charge over time (e.g. a subscription).
/// Ordinals kept from the earlier None/FixedCount/Indeterminate set (FixedCount became
/// ForecastMonths — same behavior) so existing rows keep their meaning.
/// </summary>
public enum DateControlRecurrenceType
{
    /// <summary>One-off entry, does not repeat.</summary>
    None,

    /// <summary>RecurrenceCount entries, one per month, all created upfront.</summary>
    ForecastMonths,

    /// <summary>No defined end: only the current occurrence exists; marking it Paid creates the
    /// next one (one month later) — see DateControlEntryService.UpdateStatusAsync.</summary>
    Indeterminate,

    /// <summary>RecurrenceCount entries, one per day, all created upfront.</summary>
    ForecastDays,
}

public enum DateControlEntryStatus
{
    Pending,
    Paid,
    Overdue,
}

/// <summary>Which reminder rule a DateControlReminderDispatch row is deduping. BeforeDue and
/// OnDue are one-off per entry (fire once, recorded here to avoid repeating on the next check
/// tick); AfterDueRepeat also gets a row per firing, but its dedupe check is "has enough time
/// passed since the last one", not "has this ever fired".</summary>
public enum DateControlReminderRuleKind
{
    BeforeDue,
    OnDue,
    AfterDueRepeat,
}

/// <summary>Whether an entry is a personal or a work expense — lets a single Financeiro module
/// hold both, filtered in the UI, instead of a separate "Trabalho" area.</summary>
public enum DateControlEntryScope
{
    Personal,
    Work,
}
