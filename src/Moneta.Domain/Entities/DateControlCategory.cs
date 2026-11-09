namespace Moneta.Domain.Entities;

/// <summary>User-editable category of a DateControl module's sources (e.g. "Internet", "Saúde").
/// Scoped per Module; Icon is the name of a Lucide icon the frontend knows how to render.</summary>
public class DateControlCategory
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public required string Name { get; set; }
    public required string Icon { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<DateControlSource> Sources { get; set; } = [];
}
