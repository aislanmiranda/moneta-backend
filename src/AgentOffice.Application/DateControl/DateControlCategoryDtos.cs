namespace AgentOffice.Application.DateControl;

/// <summary>UsageCount: how many records point at it — deleting is blocked while it's > 0.</summary>
public record DateControlCategoryDto(Guid Id, Guid ModuleId, string Name, string Icon, int UsageCount);

public record SaveDateControlCategoryDto(string Name, string Icon);
