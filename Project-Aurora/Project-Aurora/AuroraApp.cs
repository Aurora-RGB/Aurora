﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Modules;
using Aurora.Modules.ProcessMonitor;
using Aurora.Settings;
using Aurora.Settings.Controls;
using Aurora.Utils;

namespace Aurora;

public sealed class AuroraApp : IDisposable
{
    private static readonly PluginsModule PluginsModule = new();
    private static readonly IpcListenerModule IpcListenerModule = new();
    
    private readonly AuroraControlInterface _controlInterface;
    private readonly HttpListenerModule _httpListenerModule = new();
    private readonly ProcessesModule _processesModule = new();
    private readonly RazerSdkModule _razerSdkModule;
    private readonly DevicesModule _devicesModule;
    private readonly LayoutsModule _layoutsModule;

    private readonly List<AuroraModule> _modules;

    private readonly AuroraTrayIcon _trayIcon;
    private readonly bool _isSilent;

    public AuroraApp(bool isSilent)
    {
        _isSilent = isSilent;
        
        _controlInterface = new AuroraControlInterface(IpcListenerModule.IpcListener);
        _razerSdkModule = new RazerSdkModule(LightingStateManagerModule.LightningStateManager);
        _devicesModule = new DevicesModule(_razerSdkModule.RzSdkManager, _controlInterface);
        var lightingStateManagerModule = new LightingStateManagerModule(
            PluginsModule.PluginManager, IpcListenerModule.IpcListener, _httpListenerModule.HttpListener,
            _devicesModule.DeviceManager, ProcessesModule.ActiveProcessMonitor, ProcessesModule.RunningProcessMonitor
        );
        var onlineSettings = new OnlineSettings(_devicesModule.DeviceManager, ProcessesModule.RunningProcessMonitor);
        _layoutsModule = new LayoutsModule(_razerSdkModule.RzSdkManager, onlineSettings.LayoutsUpdate);
        
        _modules =
        [
            new UpdateModule(),
            new UpdateCleanup(),
            new InputsModule(),
            new MediaInfoModule(),
            new AudioCaptureModule(),
            new PointerUpdateModule(),
            new HardwareMonitorModule(),
            PluginsModule,
            IpcListenerModule,
            _httpListenerModule,
            _processesModule,
            new LogitechSdkModule(ProcessesModule.RunningProcessMonitor),
            _razerSdkModule, //depends LSM
            _devicesModule,  //depends Chroma
            lightingStateManagerModule, //depends DeviceManager
            onlineSettings,
            _layoutsModule,
            new PerformanceMonitor(ProcessesModule.RunningProcessMonitor)
        ];
        
        _trayIcon = new AuroraTrayIcon(_controlInterface);
    }

    public async Task OnStartup()
    {
        new UserSettingsBackup().BackupIfNew();
        var systemInfo = SystemUtils.GetSystemInfo();
        Global.logger.Information("{Sys}", systemInfo);

        //Load config
        Global.logger.Information("Loading Configuration");
        Global.Configuration = await ConfigManager.Load();

        Global.effengine = new Effects(_devicesModule.DeviceManager);

        if (Global.Configuration.HighPriority)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }

        WindowListener.Instance = new WindowListener();
        var initModules = _modules.Select(async m => await m.InitializeAsync())
            .Where(t => t!= null)
            .ToArray();

        _controlInterface.TrayIcon = _trayIcon.TrayIcon;
        _controlInterface.DeviceManager = await _devicesModule.DeviceManager;
        await _controlInterface.Initialize();
        _trayIcon.DisplayWindow += TrayIcon_OnDisplayWindow;
        var configUi = await CreateWindow();

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
                await m.DisposeAsync();
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
        Application.Current.Dispatcher.BeginInvoke(async () =>
        {
            if (Application.Current.MainWindow is not ConfigUI mainWindow)
            {
                var configUi = await CreateWindow();
                Application.Current.MainWindow = configUi;
                configUi.Display();
                return;
            }
            mainWindow.Display();
        });
    }

    private async Task<ConfigUI> CreateWindow()
    {
        Global.logger.Information("Loading ConfigUI...");
        var stopwatch = Stopwatch.StartNew();
        var configUi = new ConfigUI(_razerSdkModule.RzSdkManager, PluginsModule.PluginManager, _layoutsModule.LayoutManager,
            _httpListenerModule.HttpListener, IpcListenerModule.IpcListener, _devicesModule.DeviceManager,
            LightingStateManagerModule.LightningStateManager, _controlInterface);
        Global.logger.Debug("new ConfigUI() took {Elapsed} milliseconds", stopwatch.ElapsedMilliseconds);
        
        stopwatch.Restart();
        await configUi.Initialize();
        Global.logger.Debug("configUi.Initialize() took {Elapsed} milliseconds", stopwatch.ElapsedMilliseconds);
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