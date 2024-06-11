using System;

namespace AuroraRgb.Profiles.Generic;

public class GameEvent_Generic : LightEvent
{
    public override void SetGameState(IGameState new_game_state)
    {
        if (Application.Config.GameStateType != null && new_game_state.GetType() != Application.Config.GameStateType)
            return;

        _game_state = new_game_state;
        UpdateLayerGameStates();
    }

    private void UpdateLayerGameStates()
    {
        var settings = Application.Profile;
        if (settings == null)
            return;

        foreach (var lyr in settings.Layers)
            lyr.SetGameState(_game_state);

        foreach (var lyr in settings.OverlayLayers)
            lyr.SetGameState(_game_state);
    }

    public override void ResetGameState()
    {
        if (Application?.Config?.GameStateType != null)
            _game_state = (IGameState)Activator.CreateInstance(Application.Config.GameStateType);
        else
            _game_state = new EmptyGameState();

        UpdateLayerGameStates();
    }
}