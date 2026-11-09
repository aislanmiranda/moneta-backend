using AgentOffice.Domain.Enums;

namespace AgentOffice.Domain.Entities;

/// <summary>One specific occurrence of a TimeControlItem ("Vitamina D, hoje às 08:00"). Generated
/// periodically by schedule/Jobs/TimeControlOccurrenceGeneratorJob.cs from the owning item's
/// schedule — never created directly by the user. No own ModuleId: reached via TimeControlItem,
/// which already carries it — avoids a redundant column to keep in sync.</summary>
public class TimeControlOccurrence
{
    public Guid Id { get; set; }

    public Guid TimeControlItemId { get; set; }
    public TimeControlItem TimeControlItem { get; set; } = null!;

    public DateTimeOffset ScheduledAt { get; set; }
    public TimeControlOccurrenceStatus Status { get; set; } = TimeControlOccurrenceStatus.Pending;
    public DateTimeOffset? DoneAt { get; set; }
}
