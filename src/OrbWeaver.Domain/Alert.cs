using System.Collections.Immutable;

namespace OrbWeaver.Domain;

public enum AlertStatus
{
    Active,
    Resolved
}

public record Alert(
    Guid Id,
    string Name,
    string Condition,
    AlertStatus Status,
    DateTime Timestamp
);

public record Notification(string AlertName, AlertStatus NewStatus);

public static class AlertReducer
{
    public static (Alert, ImmutableList<Notification>) Reduce(Alert state, UpdateMessage update)
        => update.Evaluate(state.Condition) switch
        {
            true when state.Status == AlertStatus.Resolved =>
            (
                state with { Status = AlertStatus.Active },
                ImmutableList.Create(new Notification(state.Name, AlertStatus.Active))
            ),
            false when state.Status == AlertStatus.Active =>
            (
                state with { Status = AlertStatus.Resolved },
                ImmutableList.Create(new Notification(state.Name, AlertStatus.Resolved))
            ),
            _ => (state, ImmutableList<Notification>.Empty)
        };
}