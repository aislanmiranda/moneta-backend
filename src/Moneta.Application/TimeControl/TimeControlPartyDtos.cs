using Moneta.Domain.Enums;

namespace Moneta.Application.TimeControl;

/// <summary>UsageCount: how many records point at it — deleting is blocked while it's > 0.</summary>
public record TimeControlPartyDto(Guid Id, Guid ModuleId, string Name, PartyRelationship Relationship, int UsageCount);

public record CreateTimeControlPartyDto(Guid ModuleId, string Name, PartyRelationship Relationship);

public record UpdateTimeControlPartyDto(string Name, PartyRelationship Relationship);
