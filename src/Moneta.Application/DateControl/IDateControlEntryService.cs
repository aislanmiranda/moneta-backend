using Moneta.Domain.Enums;

namespace Moneta.Application.DateControl;

public interface IDateControlEntryService
{
    Task<List<DateControlEntryDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<MonthlyDashboardDto> GetMonthlyDashboardAsync(Guid moduleId, int year, CancellationToken ct = default);
    Task<DateControlEntryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DateControlEntryDto> CreateAsync(CreateDateControlEntryDto input, CancellationToken ct = default);
    Task<DateControlEntryDto?> UpdateAsync(Guid id, UpdateDateControlEntryDto input, CancellationToken ct = default);
    Task<DateControlEntryDto?> UpdateStatusAsync(Guid id, DateControlEntryStatus status, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);

    Task<List<DateControlEntryAttachmentDto>?> GetAttachmentsAsync(Guid entryId, CancellationToken ct = default);
    Task<DateControlEntryAttachmentDto?> AddAttachmentAsync(Guid entryId, UploadAttachmentInput input, CancellationToken ct = default);
    Task<AttachmentFile?> GetAttachmentFileAsync(Guid entryId, Guid attachmentId, CancellationToken ct = default);
    Task<bool> RemoveAttachmentAsync(Guid entryId, Guid attachmentId, CancellationToken ct = default);
}
