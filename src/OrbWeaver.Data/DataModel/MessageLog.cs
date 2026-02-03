using System.Text.Json;

namespace OrbWeaver.Data.DataModel;

public class MessageLog
{
    public required string Hash { get; init; }
    public required JsonDocument Payload { get; init; }
    public required DateTime LoggedAt { get; init; }
    public required DateTime Timestamp { get; init; }
}