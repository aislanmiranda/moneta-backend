using AgentOffice.Domain.Enums;

namespace AgentOffice.Domain.Entities;

/// <summary>A registered bill/account within a DateControl Module (e.g. "Internet residencial").
/// Entries against it live in <see cref="DateControlEntry"/>. Scoped per Module so two different
/// modules sharing the DateControl pattern don't mix their sources.</summary>
public class DateControlSource
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public required string Description { get; set; }
    public Guid DateControlCategoryId { get; set; }
    public DateControlCategory DateControlCategory { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<DateControlEntry> Entries { get; set; } = [];
}
