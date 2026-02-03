using Microsoft.Extensions.Logging;
using OrbWeaver.Application.Abstractions;
using OrbWeaver.Domain;

namespace OrbWeaver.Application.Handler;

public class UpdateHandler(
    IMessageLogRepository messageLogRepository,
    ILogger<UpdateHandler> logger)
    : IUpdateHandler
{
    public async Task Handle(string key, string rawMessage, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Logging update with key");

        var affected = await messageLogRepository.Log(new Message(rawMessage, timestamp), cancellationToken);
        
        logger.LogInformation("Update successfully logged, affected rows: {Affected}", affected);
    }
}