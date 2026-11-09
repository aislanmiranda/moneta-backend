using AgentOffice.Application.Routines;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutinesController(IRoutineService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoutineDto>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoutineDto>> GetById(Guid id, CancellationToken ct)
    {
        var routine = await service.GetByIdAsync(id, ct);
        return routine is null ? NotFound() : Ok(routine);
    }

    [HttpPost]
    public async Task<ActionResult<RoutineDto>> Create(CreateRoutineDto request, CancellationToken ct)
    {
        var routine = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = routine.Id }, routine);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoutineDto>> Update(Guid id, UpdateRoutineDto request, CancellationToken ct)
    {
        var routine = await service.UpdateAsync(id, request, ct);
        return routine is null ? NotFound() : Ok(routine);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<RoutineDto>> UpdateStatus(
        Guid id, UpdateRoutineStatusDto request, CancellationToken ct)
    {
        var routine = await service.UpdateStatusAsync(id, request.Status, ct);
        return routine is null ? NotFound() : Ok(routine);
    }

    [HttpPost("{id:guid}/trigger-now")]
    public async Task<ActionResult<RoutineDto>> TriggerNow(Guid id, CancellationToken ct)
    {
        var routine = await service.RequestTriggerAsync(id, ct);
        return routine is null ? NotFound() : Accepted(routine);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
