namespace AuroraRgb.Profiles.Generic;

public class GameEvent_Generic : LightEvent
{
    public override void SetGameState(IGameState newGameState)
    {
        if (newGameState.GetType() != Application.Config.GameStateType)
            return;

        GameState = newGameState;
        UpdateLayerGameStates();
    }

    private void UpdateLayerGameStates()
    {
        var settings = Application?.Profile;
        if (settings == null)
            return;

        foreach (var lyr in settings.Layers)
            lyr.SetGameState(GameState);

        foreach (var lyr in settings.OverlayLayers)
            lyr.SetGameState(GameState);
    }

    public override void ResetGameState()
    {
        base.ResetGameState();

        UpdateLayerGameStates();
    }
}