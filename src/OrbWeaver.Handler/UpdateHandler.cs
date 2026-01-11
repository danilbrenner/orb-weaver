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
    ILogger<UpdateHandler> logger)
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

        await Task.Delay(10, cancellationToken);

        logger.LogInformation("Update handled successfully for key: {Key}", key);
    }
}