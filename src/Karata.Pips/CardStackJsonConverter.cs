using System.Text.Json;
using System.Text.Json.Serialization;

namespace Karata.Pips;

/// <summary>Serializes a stack of cards from the bottom up, and reads it back by pushing in that order.</summary>
public sealed class CardStackJsonConverter<TStack> : JsonConverter<TStack> where TStack : Stack<Card>, new()
{
    public override TStack Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stack = new TStack();
        (JsonSerializer.Deserialize<List<Card>>(ref reader, options) ?? []).ForEach(stack.Push);

        return stack;
    }

    public override void Write(Utf8JsonWriter writer, TStack value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.Reverse(), options);
}