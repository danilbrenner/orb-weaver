using Microsoft.Extensions.Logging;

namespace OrbWeaver.Handler;

public interface IUpdateHandler
{
    Task Handle(string key, string rawUpdate, CancellationToken cancellationToken = default);
}

public class UpdateHandler(ILogger<UpdateHandler> logger) : IUpdateHandler
{
    public async Task Handle(string key, string rawUpdate, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling update with key: {Key}, rawUpdate: {RawUpdate}", key, rawUpdate);
        await Task.Delay(10, cancellationToken);
    }
}