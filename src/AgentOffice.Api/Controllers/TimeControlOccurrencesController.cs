using AgentOffice.Application.TimeControl;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/occurrences")]
public class TimeControlOccurrencesController(ITimeControlOccurrenceService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimeControlOccurrenceDto>>> GetAll(
        Guid moduleId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, from, to, ct));

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TimeControlOccurrenceDto>> UpdateStatus(
        Guid moduleId, Guid id, UpdateTimeControlOccurrenceStatusDto request, CancellationToken ct)
    {
        var occurrence = await service.UpdateStatusAsync(id, request.Status, ct);
        return occurrence is null ? NotFound() : Ok(occurrence);
    }
}
