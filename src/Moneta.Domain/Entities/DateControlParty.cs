using Moneta.Domain.Enums;

namespace Moneta.Domain.Entities;

/// <summary>Who a DateControl entry is for — myself, a family member, or a custom label.
/// Scoped per Module (was "CostCenter", Financeiro-specific).</summary>
public class DateControlParty
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public required string Name { get; set; }
    public PartyRelationship Relationship { get; set; } = PartyRelationship.Other;

    public ICollection<DateControlEntry> Entries { get; set; } = [];
}
