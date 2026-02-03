namespace OrbWeaver.Domain;

public enum AlertStatus
{
    Active,
    Inactive
}

public record AlertUpdate(bool IsResolved, DateTime Timestamp);

public record AlertState(
    Guid AlertId,
    string Name,
    AlertStatus Status,
    long Version,
    DateTime LastHandledTs,
    AlertUpdate? LatestUpdate)
{
    public (AlertState?, Notification?) HandleLatestUpdate() =>
        LatestUpdate switch
        {
            { IsResolved: true, Timestamp: var ts } when Status == AlertStatus.Active =>
            (
                this with { Status = AlertStatus.Inactive, LastHandledTs = ts },
                new Notification(Name, AlertStatus.Inactive)
            ),
            { IsResolved: false, Timestamp: var ts } when Status == AlertStatus.Inactive =>
            (
                this with { Status = AlertStatus.Active, LastHandledTs = ts },
                new Notification(Name, AlertStatus.Active)
            ),
            { Timestamp: var ts} => ( this with { LastHandledTs = ts }, null ),
            null => (null, null)
        };
}

public record Notification(string AlertName, AlertStatus NewStatus);