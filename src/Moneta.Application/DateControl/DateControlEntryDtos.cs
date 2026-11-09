using Moneta.Domain.Enums;

namespace Moneta.Application.DateControl;

public record DateControlEntryDto(
    Guid Id,
    Guid ModuleId,
    Guid DateControlSourceId,
    string DateControlSourceDescription,
    string CategoryName,
    string CategoryIcon,
    Guid DateControlPartyId,
    string DateControlPartyName,
    decimal Amount,
    DateOnly DueDate,
    int? InstallmentsCount,
    DateControlRecurrenceType RecurrenceType,
    int? RecurrenceCount,
    DateControlEntryStatus Status,
    int? OccurrenceNumber,
    DateControlEntryScope Scope,
    string? Notes,
    int AttachmentsCount
);

public record CreateDateControlEntryDto(
    Guid ModuleId,
    Guid DateControlSourceId,
    Guid DateControlPartyId,
    decimal Amount,
    DateOnly DueDate,
    int? InstallmentsCount,
    DateControlRecurrenceType RecurrenceType,
    int? RecurrenceCount,
    DateControlEntryScope Scope = DateControlEntryScope.Personal,
    string? Notes = null
);

public record UpdateDateControlEntryStatusDto(DateControlEntryStatus Status);

/// <summary>
/// Edits an already-created entry. Deliberately narrower than <see cref="CreateDateControlEntryDto"/>:
/// no DateControlSourceId/description/category (several entries in a generated series share the
/// same DateControlSource, so renaming it here would rename it for all of them) and no
/// installments/recurrence (those drove series generation at creation time; editing them on one
/// row wouldn't regenerate its siblings, which would be confusing).
/// </summary>
public record UpdateDateControlEntryDto(
    Guid DateControlPartyId,
    decimal Amount,
    DateOnly DueDate,
    DateControlEntryScope Scope = DateControlEntryScope.Personal,
    string? Notes = null
);

public record DateControlEntryAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset CreatedAt
);

/// <summary>Raw upload, decoupled from ASP.NET's IFormFile so Application stays web-agnostic.</summary>
public record UploadAttachmentInput(string FileName, string ContentType, byte[] Content);

public record AttachmentFile(string FileName, string ContentType, byte[] Content);
