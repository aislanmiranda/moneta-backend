namespace AgentOffice.Application.DateControl;

public interface IDateControlCategoryService
{
    Task<List<DateControlCategoryDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateControlCategoryDto> CreateAsync(Guid moduleId, SaveDateControlCategoryDto input, CancellationToken ct = default);
    Task<DateControlCategoryDto?> UpdateAsync(Guid id, SaveDateControlCategoryDto input, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
