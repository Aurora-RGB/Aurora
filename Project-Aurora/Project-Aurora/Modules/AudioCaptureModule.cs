﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.AudioCapture;
using AuroraRgb.Settings;
using Lombok.NET;
using NAudio.CoreAudioApi;

namespace AuroraRgb.Modules;

public sealed partial class AudioCaptureModule : AuroraModule
{
    private AudioDevices? _audioDevices;
    private AudioDeviceProxy? _renderProxy;
    private AudioDeviceProxy? _captureProxy;

    protected override Task Initialize()
    {
        InitializeLocalInfoProxies();

        Global.Configuration.PropertyChanged += ConfigurationOnAudioCaptureChanged;

        return Task.CompletedTask;
    }

    private void ConfigurationOnAudioCaptureChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Global.Configuration.EnableAudioCapture))
        {
            return;
        }

        if (Global.Configuration.EnableAudioCapture)
        {
            InitCapture();
        }
        else
        {
            DisposeCapture();
        }
    }

    private void InitializeDeviceListProxy()
    {
        _audioDevices = new AudioDevices();
    }

    private void InitializeLocalInfoProxies()
    {
        InitializeDeviceListProxy();
        try
        {
            InitRender();
        }
        catch (Exception e)
        {
            MessageBox.Show("Audio device could not be loaded.\n" +
                            "Audio information such as output level won't be updated.\n" +
                            "Cause of this could be bad drivers or bad implementation from Aurora",
                "Aurora - Warning");
            Global.logger.Error(e, "AudioCapture error");
        }

        if (!Global.Configuration.EnableAudioCapture) return;
        try
        {
            InitCapture();
        }
        catch (Exception e)
        {
            MessageBox.Show("Input audio device could not be loaded.\n" +
                            "Audio capture information such as microphone level won't be updated.\n" +
                            "Cause of this could be bad drivers or bad implementation",
                "Aurora - Warning");
            Global.logger.Error(e, "AudioCapture error");
        }
    }

    private void InitRender()
    {
        _renderProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioRenderDevice, DataFlow.Render);
        Global.Configuration.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Configuration.GsiAudioRenderDevice))
                _renderProxy.DeviceId = Global.Configuration.GsiAudioRenderDevice;
        };
        Global.RenderProxy = _renderProxy;
    }

    private void InitCapture()
    {
        _captureProxy?.Dispose();
        _captureProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioCaptureDevice, DataFlow.Capture);
        Global.Configuration.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Configuration.GsiAudioCaptureDevice))
                _captureProxy.DeviceId = Global.Configuration.GsiAudioCaptureDevice;
        };
        Global.CaptureProxy = _captureProxy;
    }

    [Async]
    public override void Dispose()
    {
        Global.Configuration.PropertyChanged -= ConfigurationOnAudioCaptureChanged;

        DisposeRender();
        DisposeCapture();

        _audioDevices?.Dispose();
        _audioDevices = null;

        AudioDeviceProxy.DisposeStatic();
    }

    private void DisposeRender()
    {
        _renderProxy?.Dispose();
        _renderProxy = null;
        Global.RenderProxy = null;
    }

    private void DisposeCapture()
    {
        _captureProxy?.Dispose();
        _captureProxy = null;
        Global.CaptureProxy = null;
    }
}