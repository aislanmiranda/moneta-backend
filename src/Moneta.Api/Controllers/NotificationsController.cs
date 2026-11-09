using Moneta.Application.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<NotificationDto>> MarkAsRead(Guid id, CancellationToken ct)
    {
        var notification = await service.MarkAsReadAsync(id, ct);
        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete(BulkDeleteNotificationsDto input, CancellationToken ct)
    {
        await service.DeleteManyAsync(input.Ids, ct);
        return NoContent();
    }
}
