namespace AgentOffice.Application.TimeControl;

/// <summary>UsageCount: how many records point at it — deleting is blocked while it's > 0.</summary>
public record TimeControlTypeDto(Guid Id, Guid ModuleId, string Name, string Icon, int UsageCount);

public record SaveTimeControlTypeDto(string Name, string Icon);
