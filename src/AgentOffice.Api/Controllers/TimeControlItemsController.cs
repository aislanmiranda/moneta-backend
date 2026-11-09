using AgentOffice.Application.TimeControl;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/items")]
public class TimeControlItemsController(ITimeControlItemService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimeControlItemDto>>> GetAll(Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpPost]
    public async Task<ActionResult<TimeControlItemDto>> Create(
        Guid moduleId, CreateTimeControlItemDto request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request with { ModuleId = moduleId }, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimeControlItemDto>> Update(
        Guid moduleId, Guid id, UpdateTimeControlItemDto request, CancellationToken ct)
    {
        var item = await service.UpdateAsync(id, request, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TimeControlItemDto>> UpdateStatus(
        Guid moduleId, Guid id, UpdateTimeControlItemStatusDto request, CancellationToken ct)
    {
        var item = await service.UpdateStatusAsync(id, request.Status, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid moduleId, Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
