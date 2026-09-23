using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AuroraRgb.Profiles.RocketLeague.GSI;

namespace AuroraRgb.Profiles.RocketLeague;

public sealed class RocketLeagueEvent : LightEvent
{
    private RlTcpSocketListener? _listener;

    public override void OnStart()
    {
        base.OnStart();

        var rlSettings = Application.Settings as RlSettings;
        var portFromSettings = rlSettings?.SocketPort ?? 49123;

        _listener = new RlTcpSocketListener(IPAddress.Loopback, portFromSettings);

        _listener.RlMessageReceived += ListenerRlMessageReceived;
        _listener.MatchCreated += ListenerOnMatchCreated;
        _listener.MatchDestroyed += ListenerOnMatchDestroyed;
        _listener.GoalScored += ListenerOnGoalScored;
        _listener.GoalReplayEnded += ListenerOnGoalReplayEnded;
        _listener.MatchEnded += ListenerOnMatchEnded;

        _ = Task.Run(async () =>
        {
            try
            {
                await _listener.StartListening();
            }
            catch (Exception ex)
            {
                Global.logger.Error(ex, "Error on Rocket League GSI listener");
            }
        });
    }

    public override void OnStop()
    {
        _listener?.RlMessageReceived -= ListenerRlMessageReceived;
        _listener?.MatchCreated -= ListenerOnMatchCreated;
        _listener?.MatchDestroyed -= ListenerOnMatchDestroyed;
        _listener?.GoalScored -= ListenerOnGoalScored;
        _listener?.GoalReplayEnded -= ListenerOnGoalReplayEnded;
        _listener?.MatchEnded += ListenerOnMatchEnded;

        base.OnStop();
    }

    public override void Dispose()
    {
        _listener?.Dispose();
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        _listener?.Dispose();
        return base.DisposeAsync();
    }

    private void ListenerRlMessageReceived(object? sender, RlMessageReceivedEventArgs e)
    {
        if (e.EventName != "UpdateState")
            return;

        if (GameState is not GameStateRocketLeague gameState)
            return;

        gameState.Data = e.Data;
    }

    private void ListenerOnMatchCreated(object? sender, EventArgs e)
    {
        if (GameState is not GameStateRocketLeague gameState)
            return;

        gameState.GameStatus = RlStatus.InGame;
        gameState.HighlightedTeam = null;
    }

    private void ListenerOnMatchDestroyed(object? sender, EventArgs e)
    {
        if (GameState is not GameStateRocketLeague gameState)
            return;

        gameState.GameStatus = RlStatus.Undefined;
    }

    private void ListenerOnGoalScored(object? sender, RlGoalScoredEventArgs e)
    {
        if (GameState is not GameStateRocketLeague gameState)
            return;

        var scorerTeamNum = e.GoalScored.Scorer.TeamNum;
        var scorerTeam = gameState.Data.Game.Teams
            .FirstOrDefault(t => t.TeamNum == scorerTeamNum);
        gameState.HighlightedTeam = scorerTeam;
    }

    private void ListenerOnGoalReplayEnded(object? sender, EventArgs e)
    {
        if (GameState is not GameStateRocketLeague gameState)
            return;

        gameState.HighlightedTeam = gameState.YourTeam;
    }

    private void ListenerOnMatchEnded(object? sender, RlMatchEndedEventArgs e)
    {
        if (GameState is not GameStateRocketLeague gameState)
            return;

        var winnerTeamNum = e.MatchEnded.WinnerTeamNum;
        var winnerTeam = gameState.Data.Game.Teams
            .FirstOrDefault(t => t.TeamNum == winnerTeamNum);
        gameState.HighlightedTeam = winnerTeam;
    }
}