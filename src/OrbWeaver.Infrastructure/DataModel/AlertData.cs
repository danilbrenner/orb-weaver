namespace OrbWeaver.Infrastructure.DataModel;

public class AlertData
{
    public Guid AlertId { get; init; }
    public required string Name { get; init; }
    public required string Filter { get; init; }
    public required string Expression { get; init; }
    public long Version { init; get; }
    public DateTime LastHandledTs { get; set; }
    public int Status { get; set; }
}