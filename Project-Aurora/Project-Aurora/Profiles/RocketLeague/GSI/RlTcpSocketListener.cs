using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using AuroraRgb.Profiles.RocketLeague.GSI.Events;

namespace AuroraRgb.Profiles.RocketLeague.GSI;

public sealed class MessageReceivedEventArgs(string message)
{
    public string Message { get; } = message;
}

public sealed class RlMessageReceivedEventArgs(string eventName, RlData data)
{
    public string EventName { get; } = eventName;
    public RlData Data { get; } = data;
}

public sealed class RlGoalScoredEventArgs(RlGoalScored goalScored)
{
    public RlGoalScored GoalScored { get; } = goalScored;
}

public sealed class RlMatchEndedEventArgs(RlMatchEnded matchEnded)
{
    public RlMatchEnded MatchEnded { get; } = matchEnded;
}

public sealed class RlTcpSocketListener(IPAddress ipAddress, int port) : IDisposable
{
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler<RlMessageReceivedEventArgs>? RlMessageReceived;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler? MatchCreated;
    public event EventHandler? MatchDestroyed;
    public event EventHandler<RlGoalScoredEventArgs>? GoalScored;
    public event EventHandler? GoalReplayEnded;
    public event EventHandler<RlMatchEndedEventArgs>? MatchEnded;

    private readonly TcpClient _client = new();

    public async Task StartListening()
    {
        await _client.ConnectAsync(ipAddress, port);
        Connected?.Invoke(this, EventArgs.Empty);

        var stream = _client.GetStream();
        var buffer = new byte[1024 * 64];

        while (true)
        {
            var read = await stream.ReadAsync(buffer);
            if (read == 0)
                break;
            var json = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(json));

            try
            {
                var rlMessage = JsonSerializer.Deserialize<RlMessage>(json, RocketLeagueJsonContext.Default.RlMessage);

                if (rlMessage is null)
                    break;

                ProcessReceivedMessage(rlMessage);
            }
            catch (JsonException ex)
            {
                // happens way too often, skip
                // Global.logger.Error(ex, $"[RocketLeagueTcp] Failed to parse JSON");
                await stream.FlushAsync();
            }
        }

        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void ProcessReceivedMessage(RlMessage m)
    {
        switch (m.EventName)
        {
            case "UpdateState":
            {
                var tickData = JsonSerializer.Deserialize<RlData>(m.Data, RocketLeagueJsonContext.Default.RlData);
                RlMessageReceived?.Invoke(this, new RlMessageReceivedEventArgs(m.EventName, tickData));
                break;
            }
            case "MatchCreated":
                MatchCreated?.Invoke(this, EventArgs.Empty);
                break;
            case "MatchDestroyed":
                MatchDestroyed?.Invoke(this, EventArgs.Empty);
                break;
            case "GoalScored":
                var scoreData = JsonSerializer.Deserialize<RlGoalScored>(m.Data, RocketLeagueJsonContext.Default.RlGoalScored);
                if (scoreData is null)
                {
                    return;
                }

                GoalScored?.Invoke(this, new RlGoalScoredEventArgs(scoreData));
                break;
            case "GoalReplayEnd":
            case "ReplayPlaybackEnd":
                GoalReplayEnded?.Invoke(this, EventArgs.Empty);
                break;
            case "MatchEnded":
                var matchEndData = JsonSerializer.Deserialize<RlMatchEnded>(m.Data, RocketLeagueJsonContext.Default.RlMatchEnded);
                if (matchEndData is null)
                {
                    return;
                }

                MatchEnded?.Invoke(this, new RlMatchEndedEventArgs(matchEndData));
                break;
            default:
                return;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}