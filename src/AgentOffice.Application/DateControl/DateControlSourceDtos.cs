namespace AgentOffice.Application.DateControl;

public record DateControlSourceDto(
    Guid Id,
    Guid ModuleId,
    string Description,
    Guid DateControlCategoryId,
    string CategoryName,
    string CategoryIcon,
    int EntriesCount
);

public record CreateDateControlSourceDto(Guid ModuleId, string Description, Guid DateControlCategoryId);

public record UpdateDateControlSourceDto(string Description, Guid DateControlCategoryId);
