using AgentOffice.Domain.Enums;

namespace AgentOffice.Domain.Entities;

/// <summary>Who a TimeControl item is for — myself or a family member. Deliberately separate
/// from DateControl's Party even though it reuses the same Relationship enum — one cadastro per
/// module, not shared data. Scoped per Module (was "HealthPerson", Vida Saudável-specific).</summary>
public class TimeControlParty
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public required string Name { get; set; }
    public PartyRelationship Relationship { get; set; } = PartyRelationship.Other;

    public ICollection<TimeControlItem> Items { get; set; } = [];
}
