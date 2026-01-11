using OrbWeaver.Domain;

namespace OrbWeaver.Tests;

public class JsonUpdateMessageTests
{
    [Fact]
    public void Evaluate_ReturnsTrue_ForExistingPrimitive()
    {
        const string json = "{ \"name\": \"orb\", \"count\": 3, \"active\": true }";
        var msg = UpdateMessage.Create(json);

        Assert.True(msg.Evaluate("$.name"));
        Assert.True(msg.Evaluate("$.count"));
        Assert.True(msg.Evaluate("$.active"));
    }

    [Fact]
    public void Evaluate_ReturnsFalse_ForMissingPath()
    {
        const string json = "{ \"name\": \"orb\" }";
        var msg = UpdateMessage.Create(json);

        Assert.False(msg.Evaluate("$.doesNotExist"));
    }

    [Fact]
    public void Evaluate_Works_ForArrays_And_Objects()
    {
        const string json = "{ \"items\": [1,2], \"emptyArray\": [], \"obj\": { \"a\": 1 }, \"emptyObj\": {} }";
        var msg = UpdateMessage.Create(json);

        Assert.True(msg.Evaluate("$.items"));
        Assert.True(msg.Evaluate("$.emptyArray"));
        Assert.True(msg.Evaluate("$.obj"));
        Assert.True(msg.Evaluate("$.emptyObj"));
    }

    [Fact]
    public void Get_ReturnsStringValue_OrNull()
    {
        const string json = "{ \"name\": \"orb\", \"num\": 5, \"nullVal\": null }";
        var msg = UpdateMessage.Create(json);

        Assert.Equal("orb", msg.Get("$.name"));
        Assert.Equal("5", msg.Get("$.num"));
        Assert.Null(msg.Get("$.doesNotExist"));
        Assert.Null(msg.Get("$.nullVal"));
    }

    [Fact]
    public void Evaluate_WithFilter_Matches()
    {
        const string json = "{ \"person\": [ { \"name\": \"David\", \"age\": 12 }, { \"name\": \"Alice\", \"age\": 9 } ] }";
        var msg = UpdateMessage.Create(json);

        Assert.True(msg.Evaluate("$.person[?(@.name == 'David')]") );
        Assert.False(msg.Evaluate("$.person[?(@.name == 'Bob')]") );
    }

    [Fact]
    public void Evaluate_WithFilter_Comparison()
    {
        const string json = "{ \"person\": [ { \"name\": \"David\", \"age\": 12 }, { \"name\": \"Alice\", \"age\": 9 }, { \"name\": \"Bob\", \"age\": 16 }  ] }";
        var msg = UpdateMessage.Create(json);

        Assert.True(msg.Evaluate("$.person[?(@.age > 10)]"));
        Assert.False(msg.Evaluate("$.person[?(@.age > 20)]"));
        Assert.False(msg.Evaluate("$.person[?(@.age > 12 && @.age < 15)]"));
        Assert.True(msg.Evaluate("$.person[?(@.age > 15 && @.age < 20)]"));
    }
    
    [Fact]
    public void Evaluate_WithFilter_NameNumericComparison_ThrowsFormatException()
    {
        // Numeric comparison against a string field (`name`) should not match typical numeric ranges.
        const string json = "{ \"person\": [ { \"name\": \"David\", \"age\": 12 }, { \"name\": \"Alice\", \"age\": 9 } ] }";
        var msg = UpdateMessage.Create(json);
    
        Assert.Throws<FormatException>(() => msg.Evaluate("$.person[?(@.name > 10 && @.name < 15)]"));
    }
}