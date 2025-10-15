using System;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuroraRgb.Utils.Json;

public class TypeAnnotatedObjectConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.FullName == typeof(Color).FullName ||
                                                        objectType.IsAssignableFrom(typeof(object));

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        writer.WriteStartObject();
        writer.WritePropertyName("$type");
        writer.WriteValue(value.GetType().AssemblyQualifiedName);
        writer.WritePropertyName("$value");
        serializer.Serialize(writer, value);
        writer.WriteEndObject();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var readerTokenType = reader.TokenType;
        if (readerTokenType == JsonToken.StartObject)
        {
            return ParseJsonNode(reader, objectType, serializer);
        }
        
        var json = reader.Value?.ToString();
        return ReadToken(json, objectType, existingValue, readerTokenType);
    }

    private static object? ParseJsonNode(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
        var item = serializer.Deserialize<JObject>(reader);
        var jToken = item?["$type"];
        if (item == null || jToken == null)
        {
            throw new JsonReaderException("item null");
        }
        var type = serializer.Deserialize<Type>(jToken.CreateReader());
        if (type == null)
        {
            //log and throw
            Global.logger.Error("$type is null. Item: {Item}", item);
            throw new JsonReaderException("$type is null");
        }
        var value = item["$value"];
        if (value == null)
        {
            return serializer.Deserialize(item.CreateReader(), type);
        }

        var valueReader = value.CreateReader();
        switch (valueReader.TokenType)
        {
            case JsonToken.StartObject:
                return serializer.Deserialize(valueReader, type);
            default:
                var s = value.ToString();
                if (type == typeof(bool) || type == typeof(Color))
                {
                    var colorString = value.ToString();
                    if (colorString.StartsWith('"'))
                    {
                        return JsonConvert.DeserializeObject(value.ToString(), type);
                    }
                    return JsonConvert.DeserializeObject("\"" + value + "\"", type);
                }
                if (objectType.FullName != typeof(Color).FullName && type.FullName != typeof(Color).FullName)
                    return ReadToken(s, type, null, valueReader.TokenType);
                if (s.StartsWith('\"'))
                {
                    return JsonConvert.DeserializeObject(s, type);
                }

                Global.logger.Error("Attempting to convert unknown type: {Type}", type);
                return JsonConvert.DeserializeObject("\"" + value + "\"", type);
        }
    }

    private static object? ReadToken(string? json, Type objectType, object? existingValue, JsonToken readerTokenType)
    {
        if (json == null)
        {
            return existingValue;
        }
        switch (readerTokenType)
        {
            case JsonToken.String:
                return json.StartsWith('\"')
                    ? JsonConvert.DeserializeObject(json, objectType)
                    : JsonConvert.DeserializeObject("\"" + json + "\"", objectType);
            case JsonToken.Integer:
                return long.TryParse(json, out var intResult)
                    ? Convert.ChangeType(intResult, objectType) 
                    : existingValue;
            case JsonToken.Float:
                return double.TryParse(json, out var result) 
                    ? Convert.ChangeType(result, objectType) 
                    : existingValue;
            case JsonToken.Boolean:
                return json.ToLowerInvariant() switch
                {
                    "true" => true,
                    "false" => false,
                    _ => existingValue
                };
            case JsonToken.Null:
                return existingValue;
        }

        return JsonConvert.DeserializeObject(json, objectType) ?? existingValue;
    }
}