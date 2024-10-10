using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonException = System.Text.Json.JsonException;

namespace AuroraRgb.Utils.Json;

public abstract class GenericDictionaryJsonConverterAdapter<K, V> : JsonConverter<IDictionary<K, V>>
    where K : notnull
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, IDictionary<K, V>? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override IDictionary<K, V> ReadJson(JsonReader reader, Type objectType, IDictionary<K, V>? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var genericTypes = objectType.GenericTypeArguments;
        var keyType = genericTypes[0];
        var valueType = genericTypes[1];

        var map = existingValue ?? (objectType.IsAbstract ?  new Dictionary<K, V>() : Instance(objectType));
            
        var item = serializer.Deserialize<JObject>(reader);
        foreach (var prop in item.Children<JProperty>())
        {
            if (prop.Name.Equals("$type"))
            {
                continue;
            }

            var key = Convert.ChangeType(prop.Name, keyType, CultureInfo.InvariantCulture);
            if (keyType == typeof(double))
            {
                double.TryParse(prop.Name, CultureInfo.InvariantCulture, out var doubleKey);
                key = doubleKey;
            }
            var value = serializer.Deserialize(prop.Value.CreateReader(), valueType);
            map.TryAdd((K)key, (V?)value ?? default(V));
        }
        return map;
    }

    private static IDictionary<K, V> Instance(Type objectType)
    {
        var constructorInfo = objectType.GetConstructor(Type.EmptyTypes);
        if (constructorInfo == null)
        {
            throw new JsonException("Type " + objectType + " does not have parameterless constructor");
        }
        return (IDictionary<K, V>)Activator.CreateInstance(objectType)!;
    }
}

public class ObjectDictionaryJsonConverterAdapter : GenericDictionaryJsonConverterAdapter<dynamic, dynamic>;

public class StringDictionaryJsonConverterAdapter : GenericDictionaryJsonConverterAdapter<string, dynamic>;

public class SingleDictionaryJsonConverterAdapter : GenericDictionaryJsonConverterAdapter<float, dynamic>;

public class DoubleDictionaryJsonConverterAdapter<T> : GenericDictionaryJsonConverterAdapter<double, T>;

public class SortedDictionaryAdapter : JsonConverter<SortedDictionary<double, Color>>
{
    private readonly DoubleDictionaryJsonConverterAdapter<Color> _genericAdapter = new();
    public override bool CanWrite => _genericAdapter.CanWrite;

    public override void WriteJson(JsonWriter writer, SortedDictionary<double, Color>? value, JsonSerializer serializer)
    {
        _genericAdapter.WriteJson(writer, value, serializer);
    }

    public override SortedDictionary<double, Color> ReadJson(JsonReader reader, Type objectType, SortedDictionary<double, Color>? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var readJson = _genericAdapter.ReadJson(reader, objectType, existingValue, hasExistingValue, serializer);
        return new SortedDictionary<double, Color>(readJson);
    }
}