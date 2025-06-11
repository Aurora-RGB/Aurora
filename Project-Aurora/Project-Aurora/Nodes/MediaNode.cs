using AuroraRgb.Modules.Media;

namespace AuroraRgb.Nodes;

public class MediaNode : Node
{
    public static bool MediaPlaying => MediaMonitor.MediaPlaying;
    public static bool HasMedia => MediaMonitor.HasMedia;
    public static bool HasNextMedia => MediaMonitor.HasNextMedia;
    public static bool HasPreviousMedia => MediaMonitor.HasPreviousMedia;
}