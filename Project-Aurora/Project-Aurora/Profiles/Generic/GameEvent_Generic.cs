using System;

namespace AuroraRgb.Profiles.Generic;

public class GameEvent_Generic : LightEvent
{
    public override void SetGameState(IGameState new_game_state)
    {
        if (Application.Config.GameStateType != null && new_game_state.GetType() != Application.Config.GameStateType)
            return;

        GameState = new_game_state;
        UpdateLayerGameStates();
    }

    private void UpdateLayerGameStates()
    {
        var settings = Application.Profile;
        if (settings == null)
            return;

        foreach (var lyr in settings.Layers)
            lyr.SetGameState(GameState);

        foreach (var lyr in settings.OverlayLayers)
            lyr.SetGameState(GameState);
    }

    public override void ResetGameState()
    {
        if (Application?.Config?.GameStateType != null)
            GameState = (IGameState)Activator.CreateInstance(Application.Config.GameStateType);
        else
            GameState = new EmptyGameState();

        UpdateLayerGameStates();
    }
}