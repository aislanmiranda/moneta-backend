namespace Moneta.Application.TimeControl;

public interface ITimeControlTypeService
{
    Task<List<TimeControlTypeDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<TimeControlTypeDto> CreateAsync(Guid moduleId, SaveTimeControlTypeDto input, CancellationToken ct = default);
    Task<TimeControlTypeDto?> UpdateAsync(Guid id, SaveTimeControlTypeDto input, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
