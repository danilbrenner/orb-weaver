using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using OrbWeaver.Domain;
using OrbWeaver.Handler.Abstractions;

namespace OrbWeaver.Handler;

public interface IUpdateHandler
{
    Task Handle(string key, string rawUpdate, CancellationToken cancellationToken = default);
}

public class UpdateHandler(
    IUpdateLogRepository updateLogRepository,
    IAlertStateRepository alertStateRepository,
    ILogger<UpdateHandler> logger,
    INotificationDispatcher notificationDispatcher)
    : IUpdateHandler
{
    public async Task Handle(string key, string rawUpdate, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling update with key: {Key}, rawUpdate: {RawUpdate}", key, rawUpdate);

        var update = UpdateMessage.Create(rawUpdate);

        var affected = await updateLogRepository.Log(update, cancellationToken);

        if (affected == 0)
        {
            logger.LogInformation("Skipping duplicate update, hash = {Hash}", update.Hash);
            return;
        }

        var alerts = await alertStateRepository.GetState(key, cancellationToken);

        var (newAlerts, notificationActions) =
            alerts.Select(alert => AlertReducer.Reduce(alert, update))
                 .Aggregate(
                     (ImmutableList<Alert>.Empty, ImmutableList<Notification>.Empty),
                     (acc, pair) => (acc.Item1.Add(pair.Item1), acc.Item2.AddRange(pair.Item2))
                 );

        await alertStateRepository.SaveState(newAlerts, cancellationToken);
        await notificationDispatcher.Dispatch(notificationActions, cancellationToken);

        logger.LogInformation("Update handled successfully for key: {Key}", key);
    }
}