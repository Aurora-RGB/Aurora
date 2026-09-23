using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.RocketLeague.GSI;

[method: JsonConstructor]
public class RlMessage(string eventName, string data)
{
    [JsonPropertyName("Event")]
    public string EventName { get; } = eventName;

    [JsonPropertyName("Data")]
    public string Data { get; } = data;
}