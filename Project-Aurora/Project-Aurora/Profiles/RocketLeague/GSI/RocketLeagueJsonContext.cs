using System.Text.Json.Serialization;
using AuroraRgb.Profiles.RocketLeague.GSI.Events;

namespace AuroraRgb.Profiles.RocketLeague.GSI;

[JsonSerializable(typeof(GameStateRocketLeague))]
[JsonSerializable(typeof(RlMessage))]
[JsonSerializable(typeof(RlGoalScored))]
[JsonSerializable(typeof(RlMatchEnded))]
public partial class RocketLeagueJsonContext : JsonSerializerContext;