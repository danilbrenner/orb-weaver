using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrbWeaver.Application.Abstractions;
using OrbWeaver.Domain;

namespace OrbWeaver.Data.Repositories;

internal class AlertStateAggregate
{
    public Guid AlertId { get; set; }
    public required string Name { get; set; }
    public int Status { get; set; }
    public long Version { get; set; }
    public DateTime LastHandledTs { get; set; }
    public bool? IsResolved { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class AlertsRepository(ILogger<AlertsRepository> logger, IDbContextFactory<OrbWeaverDbContext> contextFactory)
    : IAlertsRepository
{
    public async Task<ImmutableList<AlertState>> GetAlertStates(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await
            context
                .Database
                .SqlQuery<AlertStateAggregate>($"""
                                                select
                                                    a.alert_id,
                                                    a.name,
                                                    a.status,
                                                    a.xmin::text::bigint as version,
                                                    a.last_handled_ts,
                                                    l.payload @@ a.expression as is_resolved,
                                                    l.timestamp as timestamp
                                                from alerts a
                                                left join lateral (
                                                    select ml.payload, ml.timestamp
                                                    from messages_log ml
                                                    where ml.payload @@ a.filter
                                                      and ml.timestamp > a.last_handled_ts
                                                    order by ml.timestamp desc limit 1
                                                ) l on true;
                                                """)
                .ToListAsync(cancellationToken);

        return result.Select(a => new AlertState(
            a.AlertId,
            a.Name,
            Enum.IsDefined(typeof(AlertStatus), a.Status)
                ? (AlertStatus)a.Status
                : AlertStatus.Inactive,
            a.Version,
            a.LastHandledTs,
            a is { Timestamp: not null, IsResolved: not null }
                ? new AlertUpdate(a.IsResolved.Value, a.Timestamp.Value)
                : null
        )).ToImmutableList();
    }

    public async Task UpdateAlertStates(ImmutableList<AlertState> alerts, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var alertsDict = alerts.ToDictionary(a => a.AlertId, a => a);

        var alertData =
            await context
                .Alerts
                .Where(a => alertsDict.Keys.Contains(a.AlertId))
                .ToListAsync(cancellationToken);

        foreach (var alertD in alertData)
        {
            var alert = alertsDict[alertD.AlertId];
            if (alertD.Version != alert.Version)
            {
                logger.LogWarning("Concurrency conflict when updating alert {AlertId}. Skipping update", alert.AlertId);
                continue;
            }

            alertD.Status = (int)alert.Status;
            alertD.LastHandledTs = alert.LastHandledTs;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}