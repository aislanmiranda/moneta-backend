using Moneta.Application.TimeControl;
using Microsoft.AspNetCore.Mvc;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/time-types")]
public class TimeControlTypesController(ITimeControlTypeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimeControlTypeDto>>> GetAll(Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpPost]
    public async Task<ActionResult<TimeControlTypeDto>> Create(Guid moduleId, SaveTimeControlTypeDto request, CancellationToken ct) =>
        Ok(await service.CreateAsync(moduleId, request, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimeControlTypeDto>> Update(Guid moduleId, Guid id, SaveTimeControlTypeDto request, CancellationToken ct)
    {
        var entity = await service.UpdateAsync(id, request, ct);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid moduleId, Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
