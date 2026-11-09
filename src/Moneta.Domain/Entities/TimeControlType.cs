namespace Moneta.Domain.Entities;

/// <summary>User-editable kind of a TimeControl module's items (e.g. "Remédio", "Água"). Reminders
/// are grouped by it (see schedule/Jobs/TimeControlReminderJob.cs). Scoped per Module; Icon is
/// the name of a Lucide icon the frontend knows how to render.</summary>
public class TimeControlType
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public required string Name { get; set; }
    public required string Icon { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<TimeControlItem> Items { get; set; } = [];
}
