using AuroraRgb.Modules.Media;
using AuroraRgb.Profiles;

namespace AuroraRgb.Nodes;

[GameStateDescription(Description)]
public class MediaNode : Node
{
    public static MediaNode Instance { get; } = new();

    private const string Description = """
                                       Data is provided by Dubya.WindowsMediaController
                                       """;

    public static bool MediaPlaying => MediaMonitor.MediaPlaying;
    public static bool HasMedia => MediaMonitor.HasMedia;
    public static bool HasNextMedia => MediaMonitor.HasNextMedia;
    public static bool HasPreviousMedia => MediaMonitor.HasPreviousMedia;

    private MediaNode()
    {
    }
}