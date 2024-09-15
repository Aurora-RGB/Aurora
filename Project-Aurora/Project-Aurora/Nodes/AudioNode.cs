using AuroraRgb.Modules.AudioCapture;

namespace AuroraRgb.Nodes;

public class AudioNode : Node
{
    private AudioDeviceProxy? CaptureDevice => Global.CaptureProxy;

    private AudioDeviceProxy? RenderDevice => Global.RenderProxy;

    /// <summary>
    /// Current system volume (as set from the speaker icon)
    /// </summary>
    // Note: Manually checks if muted to return 0 since this is not taken into account with the MasterVolumeLevelScalar.
    public float SystemVolume => SystemVolumeIsMuted ? 0 : RenderDevice?.Volume ?? 0 * 100;

    /// <summary>
    /// Gets whether the system volume is muted.
    /// </summary>
    public bool SystemVolumeIsMuted => RenderDevice?.IsMuted ?? true;

    /// <summary>
    /// The volume level that is being recorded by the default microphone even when muted.
    /// </summary>
    public float MicrophoneLevel => CaptureDevice?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// The volume level that is being recorded by the default microphone even when muted.
    /// </summary>
    public float MicrophoneVolume => CaptureDevice?.Volume ?? 0 * 100;

    /// <summary>
    /// The volume level that is being emitted by the default speaker even when muted.
    /// </summary>
    public float SpeakerLevel => RenderDevice?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// The volume level that is being recorded by the default microphone if not muted.
    /// </summary>
    public float MicLevelIfNotMuted => MicrophoneIsMuted ? 0 : CaptureDevice?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// Gets whether the default microphone is muted.
    /// </summary>
    public bool MicrophoneIsMuted => CaptureDevice?.IsMuted ?? true;

    /// <summary>
    /// Selected Audio Device's index.
    /// </summary>
    public string PlaybackDeviceName => RenderDevice?.DeviceName ?? string.Empty;

    /// <summary>
    /// Selected Audio Device's index.
    /// </summary>
    public string RecordingDeviceName => CaptureDevice?.DeviceName ?? string.Empty;
}