using AgentOffice.Domain.Enums;

namespace AgentOffice.Application.DateTimeControl;

public interface IDateTimeControlEntryService
{
    Task<List<DateTimeControlEntryDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateTimeControlEntryDto> CreateAsync(CreateDateTimeControlEntryDto input, CancellationToken ct = default);
    Task<DateTimeControlEntryDto?> UpdateAsync(Guid id, UpdateDateTimeControlEntryDto input, CancellationToken ct = default);
    Task<DateTimeControlEntryDto?> UpdateStatusAsync(Guid id, DateTimeControlEntryStatus status, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
