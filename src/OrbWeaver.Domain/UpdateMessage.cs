using System.Security.Cryptography;
using System.Text;

namespace OrbWeaver.Domain;

public record UpdateMessage
{
    public string Payload { get; }
    public string Hash { get; }
    
    private UpdateMessage(string payload, string hash)
    {
        Payload = payload;
        Hash = hash;
    }
    
    public static UpdateMessage Create(string message)
        => new (message, ComputeSha256(message));
    
    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}