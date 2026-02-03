namespace OrbWeaver.Application.Abstractions;

public interface IUpdateHandler
{
    Task Handle(string key, string rawMessage, DateTime timestamp, CancellationToken cancellationToken = default);
}