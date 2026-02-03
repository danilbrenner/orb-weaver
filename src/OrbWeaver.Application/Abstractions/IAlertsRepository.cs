using System.Collections.Immutable;
using OrbWeaver.Domain;

namespace OrbWeaver.Application.Abstractions;

public interface IAlertsRepository
{
    Task<ImmutableList<AlertState>> GetAlertStates(CancellationToken cancellationToken = default);
    Task UpdateAlertStates(ImmutableList<AlertState> alerts, CancellationToken cancellationToken = default);
}