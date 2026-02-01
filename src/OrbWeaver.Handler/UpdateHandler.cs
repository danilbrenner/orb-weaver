using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using OrbWeaver.Domain;
using OrbWeaver.Handler.Abstractions;

namespace OrbWeaver.Handler;

public interface IUpdateHandler
{
    Task Handle(string key, string rawMessage, CancellationToken cancellationToken = default);
}

public class UpdateHandler(
    IMessageLogRepository messageLogRepository,
    ILogger<UpdateHandler> logger)
    : IUpdateHandler
{
    public async Task Handle(string key, string rawMessage, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Logging update with key");

        _ = await messageLogRepository.Log(new Message(rawMessage), cancellationToken);
        
        logger.LogInformation("Update successfully logged");
    }
}