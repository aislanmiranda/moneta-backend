using AgentOffice.Domain.Enums;

namespace AgentOffice.Application.Routines;

public interface IRoutineService
{
    Task<List<RoutineDto>> GetAllAsync(CancellationToken ct = default);
    Task<RoutineDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RoutineDto> CreateAsync(CreateRoutineDto input, CancellationToken ct = default);
    Task<RoutineDto?> UpdateAsync(Guid id, UpdateRoutineDto input, CancellationToken ct = default);
    Task<RoutineDto?> UpdateStatusAsync(Guid id, RoutineStatus status, CancellationToken ct = default);
    Task<RoutineDto?> RequestTriggerAsync(Guid id, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
