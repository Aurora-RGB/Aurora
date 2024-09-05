using System.Text.Json.Serialization;
using AuroraRgb.Profiles.Payday_2.GSI.Nodes;

namespace AuroraRgb.Profiles.Payday_2.GSI;

/// <summary>
/// A class representing various information retaining to Payday 2
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public partial class GameState_PD2 : GameState
{
    /// <summary>
    /// Information about the provider of this GameState
    /// </summary>
    public ProviderPd2 Provider { get; set; } = ProviderPd2.Default;

    /// <summary>
    /// Information about the game lobby
    /// </summary>
    public LobbyNodePd2 Lobby { get; set; } = LobbyNodePd2.Default;

    /// <summary>
    /// Information about the game level
    /// </summary>
    public LevelNodePd2 Level { get; set; } = LevelNodePd2.Default;

    /// <summary>
    /// Information about the local player
    /// </summary>
    public PlayerNodePd2 LocalPlayer => Players.LocalPlayer;

    /// <summary>
    /// Information about players in the lobby
    /// </summary>
    [Range(0, 3)]
    public PlayersNodePd2 Players { get; set; } = PlayersNodePd2.Default;

    /// <summary>
    /// Information about the game
    /// </summary>
    public GameNodePd2 Game { get; set; } = GameNodePd2.Default;

    /// <summary>
    ///  A previous GameState
    /// </summary>
    [JsonPropertyName("previous")]
    public GameState_PD2? Previously { get; set; }
}