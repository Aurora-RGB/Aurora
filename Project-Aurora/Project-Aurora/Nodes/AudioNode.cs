using NAudio.CoreAudioApi;

namespace AuroraRgb.Nodes;

public class AudioNode : Node
{
    private MMDevice? CaptureDevice => Global.CaptureProxy?.Device;

    private MMDevice? RenderDevice => Global.RenderProxy?.Device;

    /// <summary>
    /// Current system volume (as set from the speaker icon)
    /// </summary>
    // Note: Manually checks if muted to return 0 since this is not taken into account with the MasterVolumeLevelScalar.
    public float SystemVolume => SystemVolumeIsMuted ? 0 : RenderDevice?.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 0 * 100;

    /// <summary>
    /// Gets whether the system volume is muted.
    /// </summary>
    public bool SystemVolumeIsMuted => RenderDevice?.AudioEndpointVolume?.Mute ?? true;

    /// <summary>
    /// The volume level that is being recorded by the default microphone even when muted.
    /// </summary>
    public float MicrophoneLevel => CaptureDevice?.AudioMeterInformation?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// The volume level that is being emitted by the default speaker even when muted.
    /// </summary>
    public float SpeakerLevel => RenderDevice?.AudioMeterInformation?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// The volume level that is being recorded by the default microphone if not muted.
    /// </summary>
    public float MicLevelIfNotMuted => MicrophoneIsMuted ? 0 : CaptureDevice?.AudioMeterInformation?.MasterPeakValue ?? 0 * 100;

    /// <summary>
    /// Gets whether the default microphone is muted.
    /// </summary>
    public bool MicrophoneIsMuted => CaptureDevice?.AudioEndpointVolume.Mute ?? true;

    /// <summary>
    /// Selected Audio Device's index.
    /// </summary>
    public string PlaybackDeviceName => Global.RenderProxy?.DeviceName ?? string.Empty;

    /// <summary>
    /// Selected Audio Device's index.
    /// </summary>
    public string RecordingDeviceName => Global.CaptureProxy?.DeviceName ?? string.Empty;
}