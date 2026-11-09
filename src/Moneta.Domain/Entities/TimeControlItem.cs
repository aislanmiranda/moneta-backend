using Moneta.Domain.Enums;

namespace Moneta.Domain.Entities;

/// <summary>A recurring thing to track by time of day, within a TimeControl Module (e.g. a
/// medication, a hydration reminder, a gym slot in Vida Saudável). Always belongs to exactly one
/// TimeControlParty (unlike DateControl's Source, which can serve different parties over time) —
/// "Vitamina D" for me and for Mom are two separate items. Owns its schedule directly (flattened,
/// not a separate table) since it's always 1:1, same as Routine.</summary>
public class TimeControlItem
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public required string Title { get; set; }
    public Guid TimeControlTypeId { get; set; }
    public TimeControlType TimeControlType { get; set; } = null!;

    public Guid TimeControlPartyId { get; set; }
    public TimeControlParty TimeControlParty { get; set; } = null!;

    /// <summary>Optional detail, e.g. a medication's dosage ("1 comprimido", "10ml").</summary>
    public string? Dosage { get; set; }

    public TimeControlItemStatus Status { get; set; } = TimeControlItemStatus.Active;

    // Same shape as Routine — composed/validated via the same CronBuilder.
    public required string CronExpression { get; set; }
    public string? DaysOfWeek { get; set; }
    public string? Hours { get; set; }
    public int Minute { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<TimeControlOccurrence> Occurrences { get; set; } = [];
}
