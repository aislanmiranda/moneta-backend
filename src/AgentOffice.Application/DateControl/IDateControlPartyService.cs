namespace AgentOffice.Application.DateControl;

public interface IDateControlPartyService
{
    Task<List<DateControlPartyDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateControlPartyDto> CreateAsync(CreateDateControlPartyDto input, CancellationToken ct = default);
    Task<DateControlPartyDto?> UpdateAsync(Guid id, UpdateDateControlPartyDto input, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
