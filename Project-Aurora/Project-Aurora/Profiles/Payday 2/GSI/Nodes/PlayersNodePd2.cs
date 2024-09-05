using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Payday_2.GSI.Nodes;

/// <summary>
/// Information about players in the lobby
/// </summary>
public class PlayersNodePd2
{
    public static readonly PlayersNodePd2 Default = new(PlayerNodePd2.Default, null, null, null);

    [JsonPropertyName("1")]
    public PlayerNodePd2 Player1 { get; }

    [JsonPropertyName("2")]
    public PlayerNodePd2? Player2 { get; }

    [JsonPropertyName("3")]
    public PlayerNodePd2? Player3 { get; }

    [JsonPropertyName("4")]
    public PlayerNodePd2? Player4 { get; }

    private List<PlayerNodePd2> Players { get; }

    /// <summary>
    /// Amount of players in the lobby
    /// </summary>
    public int Count => Players.Count;

    /// <summary>
    /// The local player
    /// </summary>
    public PlayerNodePd2 LocalPlayer { get; }

    public PlayersNodePd2(PlayerNodePd2 player1, PlayerNodePd2? player2, PlayerNodePd2? player3, PlayerNodePd2? player4)
    {
        Player1 = player1;
        Player2 = player2 ?? PlayerNodePd2.Default;
        Player3 = player3 ?? PlayerNodePd2.Default;
        Player4 = player4 ?? PlayerNodePd2.Default;

        Players = [Player1, Player2, Player3, Player4];
        LocalPlayer = Players.FirstOrDefault(p => p.IsLocalPlayer, PlayerNodePd2.Default);
    }

    /// <summary>
    /// Gets the player at a selected index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public PlayerNodePd2 this[int index]
    {
        get
        {
            if (index > Players.Count - 1)
            {
                return PlayerNodePd2.Default;
            }

            return Players[index];
        }
    }

    public IEnumerator GetEnumerator()
    {
        return Players.GetEnumerator();
    }
}