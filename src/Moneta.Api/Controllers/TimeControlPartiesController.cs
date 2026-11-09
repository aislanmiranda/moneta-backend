using Moneta.Application.TimeControl;
using Microsoft.AspNetCore.Mvc;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/time-parties")]
public class TimeControlPartiesController(ITimeControlPartyService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimeControlPartyDto>>> GetAll(Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpPost]
    public async Task<ActionResult<TimeControlPartyDto>> Create(
        Guid moduleId, CreateTimeControlPartyDto request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request with { ModuleId = moduleId }, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimeControlPartyDto>> Update(
        Guid moduleId, Guid id, UpdateTimeControlPartyDto request, CancellationToken ct)
    {
        var party = await service.UpdateAsync(id, request, ct);
        return party is null ? NotFound() : Ok(party);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid moduleId, Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
