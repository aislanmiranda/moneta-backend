using AgentOffice.Application.DateControl;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/parties")]
public class DateControlPartiesController(IDateControlPartyService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DateControlPartyDto>>> GetAll(Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpPost]
    public async Task<ActionResult<DateControlPartyDto>> Create(
        Guid moduleId, CreateDateControlPartyDto request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request with { ModuleId = moduleId }, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DateControlPartyDto>> Update(
        Guid moduleId, Guid id, UpdateDateControlPartyDto request, CancellationToken ct)
    {
        var party = await service.UpdateAsync(id, request, ct);
        return party is null ? NotFound() : Ok(party);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid moduleId, Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
