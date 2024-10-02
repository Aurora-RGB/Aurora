using AuroraRgb.Profiles.Witcher3.GSI;
using Witcher3Gsi;

namespace AuroraRgb.Profiles.Witcher3;

public sealed class GameEventWitcher3 : LightEvent
{
    private readonly Witcher3GameStateListener _gameStateListener;

    //The mod this uses was taken from https://github.com/SpoinkyNL/Artemis/, with Spoinky's permission
    public GameEventWitcher3()
    {
        _gameStateListener = new Witcher3GameStateListener();
        _gameStateListener.GameStateChanged += GameStateListenerOnGameStateChanged;
    }

    public override void OnStart()
    {
        base.OnStart();
        _gameStateListener.StartReading();
    }

    public override void OnStop()
    {
        base.OnStop();
        _gameStateListener.StopListening();
    }

    private void GameStateListenerOnGameStateChanged(object? sender, Witcher3StateEventArgs e)
    {
        var player = e.GameState.Player;

        if (GameState is not GameStateWitcher3 gameState)
        {
            return;
        }

        var gameStatePlayer = gameState.Player;

        gameStatePlayer.MaximumHealth = player.MaximumHealth;
        gameStatePlayer.CurrentHealth = player.CurrentHealth;
        gameStatePlayer.Stamina = player.Stamina;
        gameStatePlayer.Toxicity = player.Toxicity;
        gameStatePlayer.ActiveSign = player.ActiveSign;
    }

    public override void Dispose()
    {
        base.Dispose();

        _gameStateListener.GameStateChanged -= GameStateListenerOnGameStateChanged;
    }
}