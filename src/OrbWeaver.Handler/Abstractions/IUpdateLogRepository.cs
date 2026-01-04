using OrbWeaver.Domain;

namespace OrbWeaver.Handler.Abstractions;

public interface IUpdateLogRepository
{
    Task<int> Log(UpdateMessage message, CancellationToken cancellationToken = default);
}