using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OrbWeaver.Domain;

public record Message
{
    public JsonDocument Payload { get; }
    public string Hash { get; }

    public Message(string raw)
    {
        Payload = JsonDocument.Parse(raw);
        Hash = ComputeSha256(raw);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}