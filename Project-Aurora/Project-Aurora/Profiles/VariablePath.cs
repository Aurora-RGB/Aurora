using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuroraRgb.Profiles;

[JsonConverter(typeof(VariablePathConverter))]
public class VariablePath
{
    public const string Seperator = "/";
    public static readonly VariablePath Empty = new("");

    private static readonly Dictionary<string, string> PathMigrations = new()
    {
        //Time
        { "LocalPCInfo/CurrentMonth",				"LocalPCInfo/Time/CurrentMonth" },
        { "LocalPCInfo/CurrentDay",					"LocalPCInfo/Time/CurrentDay" },
        { "LocalPCInfo/CurrentHour",				"LocalPCInfo/Time/CurrentHour" },
        { "LocalPCInfo/CurrentMinute",				"LocalPCInfo/Time/CurrentMinute" },
        { "LocalPCInfo/CurrentSecond",				"LocalPCInfo/Time/CurrentSecond" },
        { "LocalPCInfo/CurrentMillisecond",			"LocalPCInfo/Time/CurrentMillisecond" },
        { "LocalPCInfo/MillisecondsSinceEpoch",		"LocalPCInfo/Time/MillisecondsSinceEpoch" },
        
        //Audio
        { "LocalPCInfo/SystemVolume",				"LocalPCInfo/Audio/SystemVolume" },
        { "LocalPCInfo/SystemVolumeIsMuted",		"LocalPCInfo/Audio/SystemVolumeIsMuted" },
        { "LocalPCInfo/MicrophoneLevel",			"LocalPCInfo/Audio/MicrophoneLevel" },
        { "LocalPCInfo/SpeakerLevel",				"LocalPCInfo/Audio/SpeakerLevel" },
        { "LocalPCInfo/MicLevelIfNotMuted",			"LocalPCInfo/Audio/MicLevelIfNotMuted" },
        { "LocalPCInfo/MicrophoneIsMuted",			"LocalPCInfo/Audio/MicrophoneIsMuted" },
        { "LocalPCInfo/PlaybackDeviceName",			"LocalPCInfo/Audio/PlaybackDeviceName" },
        
        //Performance
        { "LocalPCInfo/CPUUsage",					"LocalPCInfo/CPU/Usage" },
        { "LocalPCInfo/MemoryUsed",					"LocalPCInfo/RAM/Used" },
        { "LocalPCInfo/MemoryFree",					"LocalPCInfo/RAM/Free" },
        { "LocalPCInfo/MemoryTotal",				"LocalPCInfo/RAM/Total" },
        
        { "LocalPCInfo/IsDesktopLocked",			"LocalPCInfo/Desktop/IsLocked" },
    };

    public string GsiPath { get; }

    public VariablePath(string? variablePath)
    {
        if (string.IsNullOrWhiteSpace(variablePath))
        {
            GsiPath = string.Empty;
        }
        else
        {
            GsiPath = PathMigrations.TryGetValue(variablePath, out var newPath) ? newPath : variablePath;
        }
    }

    protected bool Equals(VariablePath other)
    {
        return GsiPath == other.GsiPath;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((VariablePath)obj);
    }

    public override int GetHashCode()
    {
        return GsiPath.GetHashCode();
    }
}

public class VariablePathConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(VariablePath);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var variablePath = token.Value<string>();
        return variablePath == null ? VariablePath.Empty : new VariablePath(variablePath);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var wrapper = (VariablePath)value;
        serializer.Serialize(writer, wrapper.GsiPath);
    }
}