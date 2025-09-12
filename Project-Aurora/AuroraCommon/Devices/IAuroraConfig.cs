using System.Text.Json.Serialization;

namespace Common.Devices;

public interface IAuroraConfig
{
    [JsonIgnore]
    public string ConfigPath { get; }
}