﻿using System.Text.Json.Serialization;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.Discord.GSI;
using AuroraRgb.Profiles.Dota_2.GSI;
using AuroraRgb.Profiles.Payday_2.GSI;
using AuroraRgb.Profiles.Stationeers.GSI;

namespace AuroraRgb.Profiles;

[JsonSerializable(typeof(GameStateCsgo))]
[JsonSerializable(typeof(GameStateDiscord))]
[JsonSerializable(typeof(GameStateDota2))]
[JsonSerializable(typeof(GameState_PD2))]
[JsonSerializable(typeof(GameStateStationeers))]
public partial class GameStateSourceGenerationContext : JsonSerializerContext;