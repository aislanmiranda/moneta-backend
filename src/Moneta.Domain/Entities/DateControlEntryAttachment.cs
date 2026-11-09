namespace Moneta.Domain.Entities;

/// <summary>A file attached to a DateControlEntry — typically the payment receipt (comprovante).
/// Stored in Postgres (bytea) so it's covered by the same backup as the rest of the data; kept in
/// its own table so listing entries never loads file contents.</summary>
public class DateControlEntryAttachment
{
    public Guid Id { get; set; }

    public Guid DateControlEntryId { get; set; }
    public DateControlEntry DateControlEntry { get; set; } = null!;

    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public required byte[] Content { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
