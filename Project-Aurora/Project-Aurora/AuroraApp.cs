﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls;
using AuroraRgb.Utils;

namespace AuroraRgb;

public sealed class AuroraApp : IDisposable
{
    public AuroraControlInterface ControlInterface { get; }

    private static readonly UpdateModule UpdateModule = new();
    private static readonly PluginsModule PluginsModule = new();
    private static readonly IpcListenerModule IpcListenerModule = new();

    private readonly HttpListenerModule _httpListenerModule = new();
    private readonly ProcessesModule _processesModule = new();
    private readonly DevicesModule _devicesModule;
    private readonly LayoutsModule _layoutsModule;

    private readonly List<AuroraModule> _modules;

    private readonly AuroraTrayIcon _trayIcon;
    private readonly bool _isSilent;

    public AuroraApp(bool isSilent)
    {
        _isSilent = isSilent;
        
        ControlInterface = new AuroraControlInterface(IpcListenerModule.IpcListener);
        _devicesModule = new DevicesModule(ControlInterface);
        var lightingStateManagerModule = new LightingStateManagerModule(
            PluginsModule.PluginManager, IpcListenerModule.IpcListener, _httpListenerModule.HttpListener,
            _devicesModule.DeviceManager, ProcessesModule.ActiveProcessMonitor, ProcessesModule.RunningProcessMonitor,
            _devicesModule
        );
        var onlineSettings = new OnlineSettings(ProcessesModule.RunningProcessMonitor);
        _layoutsModule = new LayoutsModule(RazerSdkModule.RzSdkManager, onlineSettings.LayoutsUpdate);
        
        _modules =
        [
            UpdateModule,
            new UpdateCleanup(),
            new PerformanceModeModule(),
            _devicesModule,
            new InputsModule(),
            new MediaInfoModule(),
            new AudioCaptureModule(),
            new PointerUpdateModule(),
            new HardwareMonitorModule(),
            IpcListenerModule,
            _httpListenerModule,
            _processesModule,
            new LogitechSdkModule(ProcessesModule.RunningProcessMonitor),
            new RazerSdkModule(),
            PluginsModule,
            lightingStateManagerModule,
            onlineSettings,
            _layoutsModule,
            new PerformanceMonitor(ProcessesModule.RunningProcessMonitor),
            new AutomaticGsiPatcher(),
        ];
        
        _trayIcon = new AuroraTrayIcon(ControlInterface);
    }

    public async Task OnStartup()
    {
        new UserSettingsBackup().BackupIfNew();
        var systemInfo = SystemUtils.GetSystemInfo();
        Global.logger.Information("{Sys}", systemInfo);

        //Load config
        Global.logger.Information("Loading Configuration");
        Global.Configuration = await ConfigManager.Load();
        Global.SensitiveData = await ConfigManager.LoadSensitiveData();

        WindowListener.Instance = new WindowListener();
        var initModules = _modules.Select(async m => await m.InitializeAsync())
            .Where(t => t!= null)
            .ToArray();

        ControlInterface.TrayIcon = _trayIcon.TrayIcon;
        ControlInterface.DeviceManager = await _devicesModule.DeviceManager;
        await ControlInterface.Initialize();
        _trayIcon.DisplayWindow += TrayIcon_OnDisplayWindow;
        var configUi = CreateWindow();

        Global.logger.Information("Waiting for modules...");
        await Task.WhenAll(initModules);
        Application.Current.MainWindow = configUi;
        Global.logger.Information("Modules initiated");
        if (!_isSilent)
        {
            DisplayWindow();
        }

        var ipcListener = await IpcListenerModule.IpcListener;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived += OnAuroraCommandReceived;
        }

        //move this to ProcessModule
        WindowListener.Instance.StartListening();

        //Debug Windows on Startup
        if (Global.Configuration.BitmapWindowOnStartUp)
            Window_BitmapView.Open();
        if (Global.Configuration.HttpWindowOnStartUp)
            Window_GSIHttpDebug.Open(_httpListenerModule.HttpListener);
    }

    public Task Shutdown()
    {
        var tasks = _modules.Select(async m =>
        {
            try
            {
                Global.logger.Debug("Shutting down {Module}", m.GetType().Name);
                await m.DisposeAsync();
                Global.logger.Debug("Shut down {Module}!", m.GetType().Name);
            }
            catch (Exception moduleException)
            {
                Global.logger.Fatal(moduleException,"Failed closing module {@Module}", m);
            }
        });
        return Task.WhenAll(tasks);
    }

    private void OnAuroraCommandReceived(object? sender, string e)
    {
        switch (e)
        {
            case "restore":
                DisplayWindow();
                break;
        }
    }

    private void DisplayWindow()
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (Application.Current.MainWindow is not ConfigUi mainWindow)
            {
                var configUi = CreateWindow();
                Application.Current.MainWindow = configUi;
                configUi.Display();
                return;
            }
            mainWindow.Display();
        });
    }

    private ConfigUi CreateWindow()
    {
        Global.logger.Information("Loading ConfigUI...");
        var stopwatch = Stopwatch.StartNew();
        var configUi = new ConfigUi(RazerSdkModule.RzSdkManager, PluginsModule.PluginManager, _layoutsModule.LayoutManager,
            _httpListenerModule.HttpListener, IpcListenerModule.IpcListener, _devicesModule.DeviceManager,
            LightingStateManagerModule.LightningStateManager, ControlInterface, UpdateModule);
        Global.logger.Debug("new ConfigUI() took {Elapsed}", stopwatch.Elapsed);
        
        stopwatch.Restart();

        Global.logger.Debug("configUi.Initialize() took {Elapsed}", stopwatch.Elapsed);
        stopwatch.Stop();

        return configUi;
    }

    private void TrayIcon_OnDisplayWindow(object? sender, EventArgs e)
    {
        DisplayWindow();
    }

    public void Dispose()
    {
        _trayIcon.Dispose();
    }
}