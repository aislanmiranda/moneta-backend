using Moneta.Domain.Enums;

namespace Moneta.Application.TimeControl;

public interface ITimeControlItemService
{
    Task<List<TimeControlItemDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<TimeControlItemDto> CreateAsync(CreateTimeControlItemDto input, CancellationToken ct = default);
    Task<TimeControlItemDto?> UpdateAsync(Guid id, UpdateTimeControlItemDto input, CancellationToken ct = default);
    Task<TimeControlItemDto?> UpdateStatusAsync(Guid id, TimeControlItemStatus status, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
