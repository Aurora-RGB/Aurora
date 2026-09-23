using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Utils;

namespace AuroraRgb.Profiles.RocketLeague.GSI;

public class JsonHexColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string token for color.");

        var hex = reader.GetString();
        return CommonColorUtils.ParseRgb(hex);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}