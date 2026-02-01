using OrbWeaver.Domain;

namespace OrbWeaver.Handler.Abstractions;

public interface IMessageLogRepository
{
    Task<int> Log(Message message, CancellationToken cancellationToken = default);
}