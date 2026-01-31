using OrbWeaver.Domain;

namespace OrbWeaver.Handler.Abstractions;

public interface IAlertStateRepository
{
    Task<IReadOnlyList<Alert>> GetState(string alertName, CancellationToken cancellationToken = default);
    Task SaveState(IReadOnlyList<Alert> alerts, CancellationToken cancellationToken = default);
}