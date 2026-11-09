using AgentOffice.Domain.Enums;

namespace AgentOffice.Application.DateControl;

/// <summary>UsageCount: how many records point at it — deleting is blocked while it's > 0.</summary>
public record DateControlPartyDto(Guid Id, Guid ModuleId, string Name, PartyRelationship Relationship, int UsageCount);

public record CreateDateControlPartyDto(Guid ModuleId, string Name, PartyRelationship Relationship);

public record UpdateDateControlPartyDto(string Name, PartyRelationship Relationship);
