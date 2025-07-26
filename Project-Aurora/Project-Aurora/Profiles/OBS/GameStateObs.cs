namespace AuroraRgb.Profiles.OBS;

public partial class GameStateObs : GameState
{
    public bool IsConnected { get; set; } 
    public bool IsRecording { get; set; }
    public bool IsStreaming { get; set; }
}