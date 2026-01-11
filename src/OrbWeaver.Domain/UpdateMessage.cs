using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OrbWeaver.Domain;

public abstract record UpdateMessage
{
    public abstract string Payload { get; }
    public abstract string Hash { get; }
    public abstract bool Evaluate(string path);
    public abstract string? Get(string path);
    
    public static UpdateMessage Create(string message)
        => new JsonUpdateMessage(JObject.Parse(message), ComputeSha256(message));

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

internal record JsonUpdateMessage : UpdateMessage
{
    private JObject PayloadObject { get; }

    public override string Payload => PayloadObject.ToString();
    public override string Hash { get; }

    internal JsonUpdateMessage(JObject payload, string hash)
    {
        PayloadObject = payload;
        Hash = hash;
    }

    public override bool Evaluate(string path) 
        => PayloadObject.SelectTokens(path).Any();

    public override string? Get(string path) 
        => PayloadObject.SelectToken(path)?.Value<string>();
}