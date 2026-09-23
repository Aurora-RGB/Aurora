using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;

namespace AuroraRgb.Modules.AudioCapture;

/// <summary>
/// Utility class to make it easier to manage dealing with audio devices and input.
/// Will handle the creation of devices if required. If another AudioDevice is using that device, they will share the same reference.
/// Can be hot-swapped to a different device, moving all events to the newly selected device.
/// </summary>
public sealed class AudioDeviceProxy : IDisposable, IMMNotificationClient
{
    // capacity 1, so calling threads will block if thread is busy
    // but not block if it is already available
    private static readonly BlockingCollection<Action> ThreadTasks = new(1);

    private static readonly CancellationTokenSource ThreadCancelSource = new();
    private static readonly Thread NAudioThread = new(() =>
    {
        if (ThreadCancelSource.IsCancellationRequested)
            return;
        var cancellationToken = ThreadCancelSource.Token;
        while (!ThreadCancelSource.IsCancellationRequested)
        {
            try
            {
                ThreadTasks.Take(cancellationToken).Invoke();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "Unexpected error in NAudio thread");
            }
        }
    })
    {
        Name = "NAudioThread",
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal,
    };

    static AudioDeviceProxy()
    {
        NAudioThread.SetApartmentState(ApartmentState.MTA);
        NAudioThread.Start();
    }

    private static List<AudioDeviceProxy> Instances { get; } = [];
    private static readonly MMDeviceEnumerator DeviceEnumerator = new();

    public event EventHandler<EventArgs>? DeviceChanged;

    // Stores event handlers added to the proxy, so they can easily be added and removed from the MMDevice when it changes without
    // needing to rely on the consumer manually removing and re-adding the events.
    private EventHandler<WaveInEventArgs>? _waveInDataAvailable;

    // ID of currently selected device.
    private string? _deviceId;
    private bool _defaultDeviceChanged;

    /// <summary>Creates a new reference to the default audio device with the given flow direction.</summary>
    public AudioDeviceProxy(DataFlow flow) : this(AudioDevices.DefaultDeviceId, flow)
    {
    }

    /// <summary>Creates a new reference to the audio device with the given ID with the given flow direction.</summary>
    public AudioDeviceProxy(string? deviceId, DataFlow flow)
    {
        ThreadTasks.Add(() =>
        {
            // device DCOM objects need to be accessed from the same thread
            DeviceEnumerator.RegisterEndpointNotificationCallback(this);
        });
        Flow = flow;
        DeviceId = deviceId ?? AudioDevices.DefaultDeviceId;
        
        Instances.Add(this);
    }

    /// <summary>Indicates recorded data is available on the selected device.</summary>
    /// <remarks>This event is automatically reassigned to the new device when it is swapped.</remarks>
    public event EventHandler<WaveInEventArgs> WaveInDataAvailable
    {
        add
        {
            _waveInDataAvailable += value; // Update stored event listeners
            if (WaveIn == null) return;
            WaveIn.StartRecording();
            WaveIn.DataAvailable += value; // If the device is valid, pass the event handler on
        }
        remove
        {
            _waveInDataAvailable -= value; // Update stored event listeners
            if (_waveInDataAvailable == null)
            {
                WaveIn?.StopRecording();
            }
            if (WaveIn != null)
            {
                WaveIn.DataAvailable -= value; // If the device is valid, pass the event handler on
            }
        }
    }

    public MMDevice? Device { get; private set; }
    public WasapiCapture? WaveIn { get; private set; }
    public string? DeviceName { get; private set; }

    public bool IsMuted { get; private set; }
    public float Volume { get; private set; }

    public float MasterPeakValue
    {
        get
        {
            TaskCompletionSource<float> tcs = new();
            ThreadTasks.Add(() =>
            {
                tcs.TrySetResult(Device?.AudioMeterInformation.MasterPeakValue ?? 0);
            });
            try
            {
                return tcs.Task.Result;
            }
            catch (OperationCanceledException)
            {
                return default;
            }
        }
    }

    /// <summary>Gets the currently assigned direction of this device.</summary>
    public DataFlow Flow { get; set; }

    /// <summary>Gets or sets the ID of the selected device.</summary>
    public string? DeviceId
    {
        get => _deviceId;
        set
        {
            if (_disposed) return;
            value ??= AudioDevices.DefaultDeviceId; // Ensure not-null (if null, assume default device)
            if (_deviceId == value && !(_defaultDeviceChanged && _deviceId == AudioDevices.DefaultDeviceId)) return;
            _defaultDeviceChanged = false;
            _deviceId = value;
            UpdateDevice();
        }
    }

    private static void RunOnNaudioThread(Action action)
    {
        if (Thread.CurrentThread == NAudioThread)
        {
            action();
            return;
        }

        if (!NAudioThread.IsAlive)
        {
            return;
        }

        // below line will block when NAudioThread is already busy
        ThreadTasks.Add(action);
    }

    private void UpdateDevice()
    {
        RunOnNaudioThread(() =>
        {
            // Release the current device (if any), removing any events as required
            DisposeCurrentDeviceOnThread();
            if (_disposed) return;

            // Get a new device with this ID and flow direction
            var mmDevice = _deviceId == AudioDevices.DefaultDeviceId
                ? GetDefaultAudioEndpoint() // Get default if no ID is provided
                : DeviceEnumerator.EnumerateAudioEndPoints(Flow, DeviceState.Active)
                    .FirstOrDefault(d => d.ID == DeviceId); // Otherwise, get the one with this ID

            if (mmDevice == null) return;

            SetDevice(mmDevice.ID);
        });
    }

    private MMDevice? GetDefaultAudioEndpoint()
    {
        try
        {
            return DeviceEnumerator.HasDefaultAudioEndpoint(Flow, Role.Multimedia) ? DeviceEnumerator.GetDefaultAudioEndpoint(Flow, Role.Multimedia) : null;
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Default audio device could not be found");
            return null;
        }
    }

    private void SetDevice(string deviceId)
    {
        RunOnNaudioThread(() => SetDeviceOnThread(deviceId));
    }

    private void SetDeviceOnThread(string deviceId)
    {
        var mmDevice = DeviceEnumerator.GetDevice(deviceId);
        if (mmDevice == null)
        {
            RunOnNaudioThread(DisposeCurrentDeviceOnThread);
            return;
        }

        var fallbackWaveIn = WaveIn;
        var fallbackDevice = Device;
        try
        {
            // Get a WaveIn from the device and start it, adding any events as required
            WaveIn = Flow == DataFlow.Render ? new WasapiLoopbackCapture(mmDevice) : new WasapiCapture(mmDevice);
            if (_waveInDataAvailable != null)
            {
                WaveIn.DataAvailable += _waveInDataAvailable;
            }
            WaveIn.RecordingStopped += WaveInOnRecordingStopped;

            Device = mmDevice;
            DeviceName = Device.FriendlyName;
            if (_waveInDataAvailable != null)
            {
                WaveIn.StartRecording();
            }
            fallbackWaveIn?.Dispose();
            fallbackDevice?.Dispose();

            IsMuted = Device.AudioEndpointVolume.Mute;
            Volume = Device.AudioEndpointVolume.MasterVolumeLevelScalar;
            Device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolumeOnOnVolumeNotification;

            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            if (e.Message.Equals("0x88890004"))
            {
                return;
            }
            WaveIn = fallbackWaveIn;
            Device = fallbackDevice;
            DeviceName = Device?.FriendlyName ?? "";
            DeviceChanged?.Invoke(this, EventArgs.Empty);
            Global.logger.Error(e, "Error while switching sound device");
        }
    }

    private void AudioEndpointVolumeOnOnVolumeNotification(AudioVolumeNotificationData data)
    {
        IsMuted = data.Muted;
        Volume = data.MasterVolume;
    }

    private void WaveInOnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        var audioException = e.Exception;
        if (audioException == null)
        {
            return;
        }

        if (audioException.Message.Equals("0x88890004") && Device?.State == DeviceState.Active)
        {
            SetDevice(Device.ID);
        }
        else if (Device != null)
        {
            Global.logger.Error(audioException, "Audio proxy error");
            RunOnNaudioThread(DisposeCurrentDeviceOnThread);
        }
    }

    private void DisposeCurrentDeviceOnThread()
    {
        if (WaveIn != null)
        {
            WaveIn.DataAvailable -= _waveInDataAvailable;
            WaveIn.RecordingStopped -= WaveInOnRecordingStopped;
            WaveIn.StopRecording();
            WaveIn.Dispose();
        }

        WaveIn = null;

        if (Device != null)
        {
            Device.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolumeOnOnVolumeNotification;
            Device.Dispose();
        }
        Device = null;
        DeviceName = string.Empty;

        DeviceChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        if (DeviceId != deviceId)
            return;

        switch (newState)
        {
            case DeviceState.Active:
                RunOnNaudioThread(DisposeCurrentDeviceOnThread);
                SetDevice(deviceId);
                break;
            case DeviceState.Disabled:
            case DeviceState.Unplugged:
            case DeviceState.NotPresent:
                RunOnNaudioThread(DisposeCurrentDeviceOnThread);
                break;
        }
    }

    public void OnDeviceAdded(string pwstrDeviceId)
    {
        if(string.IsNullOrEmpty(pwstrDeviceId)) return;
        if (pwstrDeviceId != DeviceId) return;
        SetDevice(pwstrDeviceId);
    }

    public void OnDeviceRemoved(string deviceId)
    {
        if (Device?.ID == deviceId && Device?.State != DeviceState.Active)
        {
            RunOnNaudioThread(DisposeCurrentDeviceOnThread);
        }
    }

    /// <summary>
    /// Update the device when changed by the system.
    /// </summary>
    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string? defaultDeviceId)
    {
        if (Flow != flow || !AudioDevices.DefaultDeviceId.Equals(DeviceId)) return;
        if (defaultDeviceId == null)
        {
            return;
        }

        SetDevice(defaultDeviceId);
    }

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
    {
        //unused
    }

    public static void DisposeStatic()
    {
        ThreadCancelSource.Cancel();
        foreach (var audioDeviceProxy in Instances.ToArray())
        {
            audioDeviceProxy.Dispose();
        }
        DeviceEnumerator.Dispose();
        ThreadCancelSource.Dispose();
    }

    #region IDisposable Implementation

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        Instances.Remove(this);
        RunOnNaudioThread(DisposeCurrentDeviceOnThread);
        RunOnNaudioThread(DisposeOnThread);
    }

    private void DisposeOnThread()
    {
        DeviceEnumerator.UnregisterEndpointNotificationCallback(this);
    }

    #endregion
}