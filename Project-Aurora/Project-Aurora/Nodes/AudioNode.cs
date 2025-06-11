using AuroraRgb.Modules.AudioCapture;

namespace AuroraRgb.Nodes;

// info by NAudio
public class AudioNode : Node
{
    public static PlaybackDeviceNode PlaybackDevice { get; } = new();
    public static RecordingDeviceNode RecordingDevice { get; } = new();
}

public class PlaybackDeviceNode : Node
{
    private static AudioDeviceProxy? RenderDevice => Global.RenderProxy;

    /// <summary>
    /// Current system volume (as set from the speaker icon)
    /// </summary>
    // Note: Manually checks if muted to return 0 since this is not taken into account with the MasterVolumeLevelScalar.
    public static float SystemVolume => SystemVolumeIsMuted ? 0 : RenderDevice?.Volume ?? 0 * 100;

    /// <summary>
    /// Gets whether the system volume is muted.
    /// </summary>
    public static bool SystemVolumeIsMuted => RenderDevice?.IsMuted ?? true;

    /// <summary>
    /// The volume level that is being emitted by the default speaker even when muted.
    /// </summary>
    public static float SpeakerLevel => RenderDevice?.MasterPeakValue ?? 0 * 100;

    public static string PlaybackDeviceName => RenderDevice?.DeviceName ?? string.Empty;
}

public class RecordingDeviceNode : Node
{
    private static AudioDeviceProxy? CaptureDevice => Global.CaptureProxy;

    /// <summary>
    /// The volume level that is being recorded by the default microphone even when muted.
    /// </summary>
    public static float MicrophoneLevel => CaptureDevice?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// The volume level that is being recorded by the default microphone even when muted.
    /// </summary>
    public static float MicrophoneVolume => CaptureDevice?.Volume ?? 0 * 100;

    /// <summary>
    /// The volume level that is being recorded by the default microphone if not muted.
    /// </summary>
    public static float MicLevelIfNotMuted => MicrophoneIsMuted ? 0 : CaptureDevice?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// Gets whether the default microphone is muted.
    /// </summary>
    public static bool MicrophoneIsMuted => CaptureDevice?.IsMuted ?? true;

    /// <summary>
    /// Selected Audio Device's index.
    /// </summary>
    public static string RecordingDeviceName => CaptureDevice?.DeviceName ?? string.Empty;
}