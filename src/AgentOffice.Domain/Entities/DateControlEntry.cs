using AgentOffice.Domain.Enums;
using AgentOffice.Domain.Exceptions;

namespace AgentOffice.Domain.Entities;

/// <summary>
/// A due-date-tracked record of a DateControl-pattern Module (e.g. a Financeiro launch): how
/// much, due when, for whom (party), and how it repeats — installments, recurrence, or a
/// forecast window.
/// </summary>
public class DateControlEntry
{
    public const int NotesMaxLength = 4000;

    /// <summary>Upper bound for ForecastDays/ForecastMonths so a typo can't create thousands of rows.</summary>
    public const int MaxForecastCount = 366;

    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public Guid DateControlSourceId { get; set; }
    public DateControlSource DateControlSource { get; set; } = null!;

    public Guid DateControlPartyId { get; set; }
    public DateControlParty DateControlParty { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }

    /// <summary>Number of installments splitting this purchase (e.g. 12x). Null/1 = à vista.</summary>
    public int? InstallmentsCount { get; set; }

    public DateControlRecurrenceType RecurrenceType { get; set; } = DateControlRecurrenceType.None;

    /// <summary>How many occurrences — required for ForecastDays/ForecastMonths only.</summary>
    public int? RecurrenceCount { get; set; }

    public bool IsForecastRecurrence => RecurrenceType is
        DateControlRecurrenceType.ForecastDays or DateControlRecurrenceType.ForecastMonths;

    /// <summary>
    /// This entry's 1-based position in its series (installments, a forecast recurrence, or the
    /// Nth generated occurrence of an indeterminate one) — e.g. 2 of "RecurrenceCount" 3. Null
    /// for a one-off entry. Each entry in a series is an independent row; there's no shared series id,
    /// so editing/paying/deleting one doesn't affect the others.
    /// </summary>
    public int? OccurrenceNumber { get; set; }

    public DateControlEntryStatus Status { get; set; } = DateControlEntryStatus.Pending;

    /// <summary>Personal or work expense — see DateControlEntryScope.</summary>
    public DateControlEntryScope Scope { get; set; } = DateControlEntryScope.Personal;

    /// <summary>Free text, e.g. the breakdown sent by whoever bills it (rent + fees...).</summary>
    public string? Notes { get; set; }

    public ICollection<DateControlEntryAttachment> Attachments { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Cross-field rules that don't fit column constraints. Called by the application service before save.</summary>
    public void EnsureValid()
    {
        if (Notes is { Length: > NotesMaxLength })
            throw new DomainValidationException($"A observação pode ter no máximo {NotesMaxLength} caracteres.");

        if (Amount <= 0)
            throw new DomainValidationException("O valor do lançamento deve ser maior que zero.");

        if (InstallmentsCount is < 1)
            throw new DomainValidationException("A quantidade de parcelas deve ser maior ou igual a 1.");

        if (IsForecastRecurrence && RecurrenceCount is null or < 1)
            throw new DomainValidationException("Informe a quantidade de lançamentos previstos.");

        if (IsForecastRecurrence && RecurrenceCount > MaxForecastCount)
            throw new DomainValidationException($"A quantidade prevista pode ser no máximo {MaxForecastCount}.");

        if (!IsForecastRecurrence && RecurrenceCount is not null)
            throw new DomainValidationException(
                "A quantidade só se aplica a lançamentos previstos em dias ou meses.");
    }
}
