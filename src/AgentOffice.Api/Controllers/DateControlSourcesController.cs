using AgentOffice.Application.DateControl;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/sources")]
public class DateControlSourcesController(IDateControlSourceService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DateControlSourceDto>>> GetAll(Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DateControlSourceDto>> GetById(Guid moduleId, Guid id, CancellationToken ct)
    {
        var source = await service.GetByIdAsync(id, ct);
        return source is null ? NotFound() : Ok(source);
    }

    [HttpPost]
    public async Task<ActionResult<DateControlSourceDto>> Create(
        Guid moduleId, CreateDateControlSourceDto request, CancellationToken ct)
    {
        var source = await service.CreateAsync(request with { ModuleId = moduleId }, ct);
        return CreatedAtAction(nameof(GetById), new { moduleId, id = source.Id }, source);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DateControlSourceDto>> Update(
        Guid moduleId, Guid id, UpdateDateControlSourceDto request, CancellationToken ct)
    {
        var source = await service.UpdateAsync(id, request, ct);
        return source is null ? NotFound() : Ok(source);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid moduleId, Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
