namespace AgentOffice.Application.DateControl;

public interface IDateControlSourceService
{
    Task<List<DateControlSourceDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateControlSourceDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DateControlSourceDto> CreateAsync(CreateDateControlSourceDto input, CancellationToken ct = default);
    Task<DateControlSourceDto?> UpdateAsync(Guid id, UpdateDateControlSourceDto input, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
