using Moneta.Api.Auth;
using Moneta.Application.Notifications;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace Moneta.Api.Realtime;

/// <summary>
/// Bridges Postgres LISTEN/NOTIFY to SignalR. Workers (see schedule/) write Notification rows
/// directly to the shared database and call `SELECT pg_notify('notifications_channel', id)`
/// right after — they never call this API. This service keeps one dedicated Npgsql connection
/// open, listens on that channel, and relays each payload as "ReceiveNotification" to the
/// connections of the user who owns it (SignalR user id = the JWT's NameIdentifier). Decouples worker from backend: neither needs to know the other is up.
/// </summary>
public class PostgresNotificationListener(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    IHubContext<NotificationsHub> hub,
    ILogger<PostgresNotificationListener> logger
) : BackgroundService
{
    private const string Channel = "notifications_channel";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ListenAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Conexão de LISTEN com o Postgres caiu, tentando de novo em 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ListenAsync(CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("Moneta"));
        await connection.OpenAsync(ct);

        connection.Notification += async (_, args) => await OnNotificationAsync(args.Payload, ct);

        await using (var cmd = new NpgsqlCommand($"LISTEN {Channel};", connection))
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }

        logger.LogInformation("Escutando notificações do Postgres (canal \"{Channel}\").", Channel);

        while (!ct.IsCancellationRequested)
        {
            await connection.WaitAsync(ct);
        }
    }

    private async Task OnNotificationAsync(string payload, CancellationToken ct)
    {
        if (!Guid.TryParse(payload, out var notificationId))
        {
            logger.LogWarning("Payload de notificação inesperado: \"{Payload}\".", payload);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        // No HTTP user here — the relay must see the row regardless of owner, then deliver it
        // only to that owner's connections.
        scope.ServiceProvider.GetRequiredService<HttpCurrentUser>().RunAsSystem();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var notification = await service.GetByIdAsync(notificationId, ct);
        if (notification is null) return;

        var ownerId = await service.GetOwnerIdAsync(notificationId, ct);
        if (ownerId is null) return;

        await hub.Clients.User(ownerId.Value.ToString()).SendAsync("ReceiveNotification", notification, ct);
    }
}
