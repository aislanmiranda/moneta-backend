using AgentOffice.Application.DateTimeControl;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/schedule-entries")]
public class DateTimeControlEntriesController(IDateTimeControlEntryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DateTimeControlEntryDto>>> GetAll(
        Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpPost]
    public async Task<ActionResult<DateTimeControlEntryDto>> Create(
        Guid moduleId, CreateDateTimeControlEntryDto request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request with { ModuleId = moduleId }, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DateTimeControlEntryDto>> Update(
        Guid id, UpdateDateTimeControlEntryDto request, CancellationToken ct)
    {
        var entry = await service.UpdateAsync(id, request, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<DateTimeControlEntryDto>> UpdateStatus(
        Guid id, UpdateDateTimeControlEntryStatusDto request, CancellationToken ct)
    {
        var entry = await service.UpdateStatusAsync(id, request.Status, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
