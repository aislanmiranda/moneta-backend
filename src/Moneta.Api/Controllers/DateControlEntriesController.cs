using Moneta.Application.DateControl;
using Microsoft.AspNetCore.Mvc;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/entries")]
public class DateControlEntriesController(IDateControlEntryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DateControlEntryDto>>> GetAll(Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DateControlEntryDto>> GetById(Guid moduleId, Guid id, CancellationToken ct)
    {
        var entry = await service.GetByIdAsync(id, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<DateControlEntryDto>> Create(
        Guid moduleId, CreateDateControlEntryDto request, CancellationToken ct)
    {
        var entry = await service.CreateAsync(request with { ModuleId = moduleId }, ct);
        return CreatedAtAction(nameof(GetById), new { moduleId, id = entry.Id }, entry);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DateControlEntryDto>> Update(
        Guid moduleId, Guid id, UpdateDateControlEntryDto request, CancellationToken ct)
    {
        var entry = await service.UpdateAsync(id, request, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<DateControlEntryDto>> UpdateStatus(
        Guid moduleId, Guid id, UpdateDateControlEntryStatusDto request, CancellationToken ct)
    {
        var entry = await service.UpdateStatusAsync(id, request.Status, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid moduleId, Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("{id:guid}/attachments")]
    public async Task<ActionResult<IEnumerable<DateControlEntryAttachmentDto>>> GetAttachments(
        Guid moduleId, Guid id, CancellationToken ct)
    {
        var attachments = await service.GetAttachmentsAsync(id, ct);
        return attachments is null ? NotFound() : Ok(attachments);
    }

    [HttpPost("{id:guid}/attachments")]
    [RequestSizeLimit(DateControlEntryService.MaxAttachmentBytes + 1024 * 1024)]
    public async Task<ActionResult<DateControlEntryAttachmentDto>> UploadAttachment(
        Guid moduleId, Guid id, IFormFile file, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);

        var attachment = await service.AddAttachmentAsync(
            id, new UploadAttachmentInput(file.FileName, file.ContentType, buffer.ToArray()), ct);
        return attachment is null ? NotFound() : Ok(attachment);
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(
        Guid moduleId, Guid id, Guid attachmentId, CancellationToken ct)
    {
        var file = await service.GetAttachmentFileAsync(id, attachmentId, ct);
        if (file is null) return NotFound();

        // Inline so PDFs/images open in the browser/phone viewer instead of forcing a download.
        Response.Headers.ContentDisposition = new System.Net.Mime.ContentDisposition
        {
            Inline = true,
            FileName = file.FileName,
        }.ToString();
        return File(file.Content, file.ContentType);
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(
        Guid moduleId, Guid id, Guid attachmentId, CancellationToken ct) =>
        await service.RemoveAttachmentAsync(id, attachmentId, ct) ? NoContent() : NotFound();
}
