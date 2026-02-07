using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using OrbWeaver.Application.Abstractions;

namespace OrbWeaver.Application.Jobs;

public class RunAlertsJob(ILogger<RunAlertsJob> logger, INotificationDispatcher notificationDispatcher, IAlertsRepository alertsRepository)
{
    public async Task Execute(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Run alertsJob started");

        var alertStates = await alertsRepository.GetAlertStates(cancellationToken);

        var results =
            alertStates.Select(a => a.HandleLatestUpdate()).ToImmutableArray();

        var notifications =
            results
                .Where(r => r.Item2 is not null)
                .Select(r => r.Item2!)
                .ToImmutableList();
        var updatedAlertStates =
            results
                .Where(r => r.Item1 is not null)
                .Select(r => r.Item1!)
                .ToImmutableList();

        await alertsRepository.UpdateAlertStates(updatedAlertStates, cancellationToken);
        if(!notifications.IsEmpty)
        {
            logger.LogInformation("Dispatching {Count} notifications", notifications.Count);
            foreach (var notification in notifications)
            {
                await notificationDispatcher.SendNotification(notification, cancellationToken);
                logger.LogInformation("Dispatched notification for alert '{AlertName}' with status '{Status}'",
                    notification.AlertName, notification.NewStatus);
            }
        }

        logger.LogInformation("Run alertsJob completed");
    }
}