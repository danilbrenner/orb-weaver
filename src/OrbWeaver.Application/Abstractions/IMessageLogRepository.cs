using OrbWeaver.Domain;

namespace OrbWeaver.Application.Abstractions;

public interface IMessageLogRepository
{
    Task<int> Log(Message message, CancellationToken cancellationToken = default);
}