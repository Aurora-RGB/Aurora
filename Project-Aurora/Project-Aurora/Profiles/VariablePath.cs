using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuroraRgb.Profiles;

[JsonConverter(typeof(VariablePathConverter))]
public sealed class VariablePath
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
        { "LocalPCInfo/SystemVolume",				"Audio/PlaybackDevice/SystemVolume" },
        { "LocalPCInfo/Audio/SystemVolume",			"Audio/PlaybackDevice/SystemVolume" },
        { "LocalPCInfo/SystemVolumeIsMuted",		"Audio/PlaybackDevice/SystemVolumeIsMuted" },
        { "LocalPCInfo/Audio/SystemVolumeIsMuted",	"Audio/PlaybackDevice/SystemVolumeIsMuted" },
        { "LocalPCInfo/MicrophoneLevel",			"Audio/RecordingDevice/MicrophoneLevel" },
        { "LocalPCInfo/Audio/MicrophoneLevel",		"Audio/RecordingDevice/MicrophoneLevel" },
        { "LocalPCInfo/SpeakerLevel",				"Audio/PlaybackDevice/SpeakerLevel" },
        { "LocalPCInfo/Audio/SpeakerLevel",			"Audio/PlaybackDevice/SpeakerLevel" },
        { "LocalPCInfo/MicLevelIfNotMuted",			"Audio/RecordingDevice/MicLevelIfNotMuted" },
        { "LocalPCInfo/Audio/MicLevelIfNotMuted",	"Audio/RecordingDevice/MicLevelIfNotMuted" },
        { "LocalPCInfo/MicrophoneIsMuted",			"Audio/RecordingDevice/MicrophoneIsMuted" },
        { "LocalPCInfo/Audio/MicrophoneIsMuted",	"Audio/RecordingDevice/MicrophoneIsMuted" },
        { "LocalPCInfo/PlaybackDeviceName",			"Audio/PlaybackDevice/PlaybackDeviceName" },
        { "LocalPCInfo/Audio/PlaybackDeviceName",	"Audio/PlaybackDevice/PlaybackDeviceName" },
 
        { "LocalPCInfo/Audio/RecordingDeviceName",	"Audio/RecordingDevice/RecordingDeviceName" },
        { "LocalPCInfo/Audio/MicrophoneVolume",		"Audio/RecordingDevice/MicrophoneVolume" },
        
        //Devices
        { "LocalPCInfo/Controllers/Controller1/IsConnected",		"Devices/Controllers/Controller1/IsConnected" },
        { "LocalPCInfo/Controllers/Controller1/Battery",		    "Devices/Controllers/Controller1/Battery" },
        { "LocalPCInfo/Controllers/Controller1/LeftTrigger",		"Devices/Controllers/Controller1/LeftTrigger" },
        { "LocalPCInfo/Controllers/Controller1/RightTrigger",		"Devices/Controllers/Controller1/RightTrigger" },
        { "LocalPCInfo/Controllers/Controller1/LeftThumbX",		    "Devices/Controllers/Controller1/LeftThumbX" },
        { "LocalPCInfo/Controllers/Controller1/LeftThumbY",		    "Devices/Controllers/Controller1/LeftThumbY" },
        { "LocalPCInfo/Controllers/Controller1/RightThumbX",		"Devices/Controllers/Controller1/RightThumbX" },
        { "LocalPCInfo/Controllers/Controller1/RightThumbY",		"Devices/Controllers/Controller1/RightThumbY" },
        
        { "LocalPCInfo/Controllers/Controller2/IsConnected",		"Devices/Controllers/Controller2/IsConnected" },
        { "LocalPCInfo/Controllers/Controller2/Battery",		    "Devices/Controllers/Controller2/Battery" },
        { "LocalPCInfo/Controllers/Controller2/LeftTrigger",		"Devices/Controllers/Controller2/LeftTrigger" },
        { "LocalPCInfo/Controllers/Controller2/RightTrigger",		"Devices/Controllers/Controller2/RightTrigger" },
        { "LocalPCInfo/Controllers/Controller2/LeftThumbX",		    "Devices/Controllers/Controller2/LeftThumbX" },
        { "LocalPCInfo/Controllers/Controller2/LeftThumbY",		    "Devices/Controllers/Controller2/LeftThumbY" },
        { "LocalPCInfo/Controllers/Controller2/RightThumbX",		"Devices/Controllers/Controller2/RightThumbX" },
        { "LocalPCInfo/Controllers/Controller2/RightThumbY",		"Devices/Controllers/Controller2/RightThumbY" },
        
        { "LocalPCInfo/Controllers/Controller3/IsConnected",		"Devices/Controllers/Controller3/IsConnected" },
        { "LocalPCInfo/Controllers/Controller3/Battery",		    "Devices/Controllers/Controller3/Battery" },
        { "LocalPCInfo/Controllers/Controller3/LeftTrigger",		"Devices/Controllers/Controller3/LeftTrigger" },
        { "LocalPCInfo/Controllers/Controller3/RightTrigger",		"Devices/Controllers/Controller3/RightTrigger" },
        { "LocalPCInfo/Controllers/Controller3/LeftThumbX",		    "Devices/Controllers/Controller3/LeftThumbX" },
        { "LocalPCInfo/Controllers/Controller3/LeftThumbY",		    "Devices/Controllers/Controller3/LeftThumbY" },
        { "LocalPCInfo/Controllers/Controller3/RightThumbX",		"Devices/Controllers/Controller3/RightThumbX" },
        { "LocalPCInfo/Controllers/Controller3/RightThumbY",		"Devices/Controllers/Controller3/RightThumbY" },
        
        { "LocalPCInfo/Controllers/Controller4/IsConnected",		"Devices/Controllers/Controller4/IsConnected" },
        { "LocalPCInfo/Controllers/Controller4/Battery",		    "Devices/Controllers/Controller4/Battery" },
        { "LocalPCInfo/Controllers/Controller4/LeftTrigger",		"Devices/Controllers/Controller4/LeftTrigger" },
        { "LocalPCInfo/Controllers/Controller4/RightTrigger",		"Devices/Controllers/Controller4/RightTrigger" },
        { "LocalPCInfo/Controllers/Controller4/LeftThumbX",		    "Devices/Controllers/Controller4/LeftThumbX" },
        { "LocalPCInfo/Controllers/Controller4/LeftThumbY",		    "Devices/Controllers/Controller4/LeftThumbY" },
        { "LocalPCInfo/Controllers/Controller4/RightThumbX",		"Devices/Controllers/Controller4/RightThumbX" },
        { "LocalPCInfo/Controllers/Controller4/RightThumbY",		"Devices/Controllers/Controller4/RightThumbY" },
        
        { "LocalPCInfo/RazerDevices/MouseBatteryPercentage",		"Devices/RazerDevices/Mouse/BatteryPercentage" },
        { "LocalPCInfo/RazerDevices/MouseBatteryCharging",		    "Devices/RazerDevices/Mouse/BatteryCharging" },
        
        //Desktop
        { "LocalPCInfo/Desktop/IsLocked",			"Desktop/IsLocked" },
        { "LocalPCInfo/Desktop/IsFocusModeEnabled",	"Desktop/IsFocusModeEnabled" },
        { "LocalPCInfo/Desktop/ActiveWindowName",	"Desktop/ActiveWindowName" },
        { "LocalPCInfo/Desktop/ActiveProcess",		"Desktop/ActiveProcess" },
        //Desktop accent colors
        { "LocalPCInfo/Desktop/AccentA",			"Desktop/AccentColorA" },
        { "LocalPCInfo/Desktop/AccentB",			"Desktop/AccentColorB" },
        { "LocalPCInfo/Desktop/AccentG",			"Desktop/AccentColorG" },
        { "LocalPCInfo/Desktop/AccentR",			"Desktop/AccentColorR" },
        
        //Desktop cursor
        { "LocalPCInfo/CursorPosition/CursorX",		"Desktop/CursorPosition/CursorX" },
        { "LocalPCInfo/CursorPosition/CursorY",		"Desktop/CursorPosition/CursorY" },
        
        //Performance
        { "LocalPCInfo/CPUUsage",					"LocalPCInfo/CPU/Usage" },
        { "LocalPCInfo/MemoryUsed",					"LocalPCInfo/RAM/Used" },
        { "LocalPCInfo/MemoryFree",					"LocalPCInfo/RAM/Free" },
        { "LocalPCInfo/MemoryTotal",				"LocalPCInfo/RAM/Total" },
        
        { "LocalPCInfo/IsDesktopLocked",			"LocalPCInfo/Desktop/IsLocked" },
        
        //Media
        { "LocalPCInfo/Media/MediaPlaying",			"Media/MediaPlaying" },
        { "LocalPCInfo/Media/HasMedia",				"Media/HasMedia" },
        { "LocalPCInfo/Media/HasNextMedia",			"Media/HasNextMedia" },
        { "LocalPCInfo/Media/HasPreviousMedia",		"Media/HasPreviousMedia" },
        
        //Celestial Data
        { "LocalPCInfo/CelestialData/SolarNoonPercentage", "CelestialData/SolarNoonPercentage" },
        
        //Processes
        { "LocalPCInfo/ActiveWindowName",			"Processes/ActiveWindowName" },
        { "LocalPCInfo/ActiveProcess",				"Processes/ActiveProcess" },
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
            GsiPath = PathMigrations.GetValueOrDefault(variablePath, variablePath);
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