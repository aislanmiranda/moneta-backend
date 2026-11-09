namespace AgentOffice.Application.TimeControl;

public interface ITimeControlPartyService
{
    Task<List<TimeControlPartyDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<TimeControlPartyDto> CreateAsync(CreateTimeControlPartyDto input, CancellationToken ct = default);
    Task<TimeControlPartyDto?> UpdateAsync(Guid id, UpdateTimeControlPartyDto input, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
