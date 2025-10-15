using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AuroraRgb.Profiles.Chroma;
using AuroraRgb.Profiles.Dota_2.Layers;
using AuroraRgb.Profiles.Generic;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Settings.Overrides.Logic.Boolean;
using Common.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace AuroraRgb.Utils.Json;

public partial class AuroraSerializationBinder : DefaultSerializationBinder
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Type>> _assemblyTypeMap = new();
    private readonly ConcurrentDictionary<string, Type> _typeMap = new();
    
    public override Type BindToType(string? assemblyName, string typeName)
    {
        ConcurrentDictionary<string, Type> typeMap;
        if (assemblyName != null)
        {
            if (assemblyName == "Aurora")
            {
                assemblyName = "AuroraRgb";
            }

            if (!_assemblyTypeMap.TryGetValue(assemblyName, out typeMap!))
            {
                typeMap = new ConcurrentDictionary<string, Type>();
                _assemblyTypeMap[assemblyName] = typeMap;
            }
        }
        else
        {
            typeMap = _typeMap;
        }

        if (typeMap.TryGetValue(typeName, out var type))
        {
            return type;
        }

        // Use the Regex.Replace method with a MatchEvaluator delegate
        var convertedTypeName = TypeAndAssemblyRegex().Replace(typeName, ReplaceGroups);
        if (convertedTypeName.Contains("Aurora]"))
        {
            convertedTypeName = convertedTypeName
                .Replace("Aurora]", "AuroraRgb]")
                .Replace("[Aurora.", "[AuroraRgb.");
        }

        var boundType = convertedTypeName switch
        {
            "System.Windows.Forms.Keys" =>
                typeof(Keys),
            "System.Collections.Generic.Queue`1[[System.Windows.Forms.Keys, System.Windows.Forms]]" =>
                typeof(List<Keys>),
            //Resolve typo'd AbilityLayerHandler type
            "Aurora.Profiles.Dota_2.Layers.Dota2AbiltiyLayerHandler" =>
                typeof(Dota2AbilityLayerHandler),
            "Aurora.Profiles.Dota_2.Layers.Dota2HeroAbiltiyEffectsLayerHandler" =>
                typeof(Dota2HeroAbilityEffectsLayerHandler),
            "Aurora.Settings.Layers.ComparisonLayerHandler" =>
                typeof(DefaultLayerHandler),
            "Aurora.Settings.Layers.ComparisonLayerProperties" =>
                typeof(LayerHandlerProperties),
            "Aurora.Profiles.Dota_2.Layers.Dota2HeroAbiltiyEffectsLayerHandlerProperties" =>
                typeof(LayerHandlerProperties2Color),
            "Aurora.Profiles.TheDivision.TheDivisionSettings" =>
                typeof(ApplicationProfile),
            "Aurora.Profiles.Overwatch.OverwatchProfile" =>
                typeof(WrapperProfile),
            "Aurora.Profiles.WormsWMD.WormsWMDProfile" =>
                typeof(WrapperProfile),
            "Aurora.Profiles.Blade_and_Soul.BnSProfile" =>
                typeof(WrapperProfile),
            "Aurora.Profiles.Magic_Duels_2012.MagicDuels2012Profile" =>
                typeof(WrapperProfile),
            "Aurora.Profiles.ColorEnhanceProfile" =>
                typeof(WrapperProfile),
            "Aurora.Settings.Overrides.Logic.IEvaluatableBoolean" =>
                typeof(Evaluatable<bool>),
            "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Boolean, mscorlib]]" =>
                typeof(Evaluatable<bool>),
            "Aurora.Settings.Overrides.Logic.IEvaluatableNumber" =>
                typeof(Evaluatable<double>),
            "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Double, mscorlib]]" =>
                typeof(Evaluatable<double>),
            "Aurora.Settings.Overrides.Logic.IEvaluatableString" =>
                typeof(Evaluatable<string>),
            "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.String, mscorlib]]" =>
                typeof(Evaluatable<string>),
            "Aurora.Settings.Overrides.Logic.Boolean.Boolean_Latch" =>
                typeof(Boolean_FlipFlopSR),
            "System.Drawing.Color" =>
                typeof(Color),
            "Aurora.Devices.DeviceKeys" =>
                typeof(DeviceKeys),
            "Common.Devices.DeviceKeys" =>
                typeof(DeviceKeys),
            "Aurora.Settings.VariableRegistry" =>
                typeof(VariableRegistry),
            "Aurora.Settings.VariableRegistryItem" =>
                typeof(VariableRegistryItem),
            "Aurora.Profiles.RazerChromaProfile" =>
                typeof(RazerChromaProfile),
            "Aurora.Profiles.WrapperProfile" =>
                typeof(WrapperProfile),
            _ => base.BindToType(assemblyName, ConvertedTypeName(convertedTypeName)),
        };
        typeMap[typeName] = boundType;

        return boundType;
    }

    public static Type? GetAuroraType(string typeName)
    {
        var regularType = Type.GetType(typeName);
        if (regularType != null)
        {
            return regularType;
        }
        var auroraType = Type.GetType(ConvertedTypeName(typeName.Split(',')[0]));
        return auroraType;
    }

    private static string ConvertedTypeName(string convertedTypeName)
    {
        if (!convertedTypeName.StartsWith("Aurora.")) return convertedTypeName;

        var regex = OldAuroraNamespace();
        return regex.Replace(convertedTypeName, "AuroraRgb.", 1);
    }

    private string ReplaceGroups(Match match)
    {
        var typeName = match.Groups[1].Value;
        var assemblyName = match.Groups[2].Value;

        var type = BindToType(assemblyName, typeName);
        var ass = type.AssemblyQualifiedName;
        var secondComma = ass.IndexOf(',', ass.IndexOf(',') + 1);
        return string.Concat("[", ass.AsSpan(0, secondComma), "]");
    }

    [GeneratedRegex(@"\[([^\s,^\[]*), ([^\s,^\]]*)]")]
    private static partial Regex TypeAndAssemblyRegex();
    [GeneratedRegex(@"Aurora\.")]
    private static partial Regex OldAuroraNamespace();
    [GeneratedRegex(@", Aurora$")]
    private static partial Regex EndsWithAurora();
}

public class EnumConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.IsEnum;

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
        // If this is a null token, then the original value was null.
        var readerTokenType = reader.TokenType;
        switch (readerTokenType)
        {
            case JsonToken.Null:
                return null;
            case JsonToken.Integer:
                return Enum.ToObject(objectType, reader.Value);
            case JsonToken.StartObject:
                var item = serializer.Deserialize<JObject>(reader);
                if (item == null)
                {
                    throw new JsonSerializationException("Enum value object is empty");
                }

                var jToken = item["$type"];
                if (jToken == null)
                {
                    throw new JsonSerializationException("Enum value type is empty");
                }

                var type = AuroraSerializationBinder.GetAuroraType(jToken.Value<string>());
                return JsonConvert.DeserializeObject(item["$value"].ToString(), type);
        }

        return existingValue;
    }
}

public class OverrideTypeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.FullName == typeof(Type).FullName;

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
        // If this is a null token, then the original value was null.
        var readerTokenType = reader.TokenType;
        switch (readerTokenType)
        {
            case JsonToken.String:
                return AuroraSerializationBinder.GetAuroraType(reader.Value.ToString());
            case JsonToken.StartObject:
                var item = serializer.Deserialize<JObject>(reader);
                foreach (var prop in item.Children<JProperty>())
                {
                    switch (prop.Name)
                    {
                        case "$type":
                            return AuroraSerializationBinder.GetAuroraType(prop.Value.ToString());
                    }
                }

                break;
            case JsonToken.Null:
                return objectType;
            default:
                return objectType;
        }

        return existingValue;
    }
}

public class SingleToDoubleConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(double);

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Float:
            case JsonToken.Integer:
                return (double)(reader.Value ?? 0.0);
            case JsonToken.String:

                double.TryParse(reader.ReadAsString(), CultureInfo.InvariantCulture, out var value);
                return value;
        }

        throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
    }
}

public class DateTimeOffsetConverterUsingDateTimeParse : System.Text.Json.Serialization.JsonConverter<DateTimeOffset >
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString() ?? "");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset  value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class VariableRegistryDictionaryConverter : JsonConverter<IDictionary<string, VariableRegistryItem>>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, IDictionary<string, VariableRegistryItem>? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override IDictionary<string, VariableRegistryItem>? ReadJson(JsonReader reader, Type objectType, IDictionary<string, VariableRegistryItem>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var readerTokenType = reader.TokenType;
        switch (readerTokenType)
        {
            case JsonToken.StartObject:
                var json = serializer.Deserialize<JObject>(reader);
                if (json == null)
                {
                    return existingValue;
                }

                var value = json["$values"];
                var valueReader = value?.CreateReader();
                if (valueReader != null)
                    return ParsePairArray(existingValue, hasExistingValue, serializer, valueReader);
                
                var map = existingValue ?? new Dictionary<string, VariableRegistryItem>();
                foreach (var prop in json.Children<JProperty>())
                {
                    if (prop.Name.Equals("$type"))
                    {
                        continue;
                    }

                    var key = prop.Name;
                    var item = serializer.Deserialize<VariableRegistryItem>(prop.Value.CreateReader())!;
                    map.TryAdd(key, item);
                }

                return map;

            case JsonToken.StartArray:
                return serializer.Deserialize(reader, typeof(IDictionary<dynamic, dynamic>)) as IDictionary<string, VariableRegistryItem>;
        }

        throw new JsonSerializationException("Unexpected json state");
    }

    private static IDictionary<string, VariableRegistryItem>? ParsePairArray(IDictionary<string, VariableRegistryItem>? existingValue, bool hasExistingValue, JsonSerializer serializer,
        JsonReader valueReader)
    {
        var list = serializer.Deserialize(valueReader, typeof(List<(string, VariableRegistryItem)>)) as List<(string, VariableRegistryItem)>;
        if (!hasExistingValue || existingValue == null) 
            return list?.ToDictionary(e => e.Item1, e => e.Item2);

        if (list == null)
        {
            return existingValue;
        }
        foreach (var valueTuple in list)
        {
            existingValue[valueTuple.Item1] = valueTuple.Item2;
        }
        return existingValue;
    }
}
