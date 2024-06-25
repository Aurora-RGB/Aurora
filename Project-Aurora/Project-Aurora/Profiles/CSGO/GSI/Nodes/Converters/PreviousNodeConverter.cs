using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes.Converters;

public class PreviousNodeConverter<TValue>(JsonSerializerOptions options) : JsonConverter<TValue>
    where TValue : class
{
    private readonly JsonConverter<TValue> _valueConverter = (JsonConverter<TValue>)options
        .GetConverter(typeof(TValue));

    // For performance, use the existing converter.

    public override TValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return null;
            case JsonTokenType.StartObject:
                return _valueConverter.Read(ref reader, typeToConvert, options);
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}