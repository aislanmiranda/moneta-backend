using Moneta.Domain.Enums;

namespace Moneta.Domain.Entities;

/// <summary>A specific-date-and-time-tracked record of a DateTimeControl-pattern Module (e.g. an
/// Agenda event): a single instant, not a recurring time-of-day (TimeControl) nor a due date with
/// no specific hour (DateControl).</summary>
public class DateTimeControlEntry
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public required string Title { get; set; }

    public DateTimeOffset ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }

    public string? Notes { get; set; }

    public DateTimeControlEntryStatus Status { get; set; } = DateTimeControlEntryStatus.Scheduled;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
