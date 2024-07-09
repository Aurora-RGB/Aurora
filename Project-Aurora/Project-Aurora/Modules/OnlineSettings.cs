using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules.OnlineConfigs;
using AuroraRgb.Modules.OnlineConfigs.Model;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Utils.IpApi;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

namespace AuroraRgb.Modules;

public sealed class OnlineSettings(Task<RunningProcessMonitor> runningProcessMonitor)
    : AuroraModule
{
    public static Dictionary<string, DeviceTooltips> DeviceTooltips { get; private set; } = new();

    public static RazerDevices RazerDeviceInfo { get; private set; } = new();

    private Dictionary<string, ShutdownProcess> _shutdownProcesses = new();
    private readonly TaskCompletionSource _layoutUpdateTaskSource = new();

    public Task LayoutsUpdate => _layoutUpdateTaskSource.Task;

    protected override async Task Initialize()
    {
        var localSettings = await OnlineConfigsRepository.GetOnlineSettingsLocal();
        var localSettingsDate = localSettings.OnlineSettingsTime;
        if (localSettingsDate > DateTimeOffset.MinValue)
        {
            // means online settings already exists, loading can continue immediately
            _layoutUpdateTaskSource.TrySetResult();
        }
        
        // reload settings as user unlocks
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        await DownloadAndExtract();
        _layoutUpdateTaskSource.TrySetResult();
        //TODO update layouts
        await Refresh();

        (await runningProcessMonitor).ProcessStarted += OnRunningProcessesChanged;

        if (Global.SensitiveData.Lat == 0 && Global.SensitiveData.Lon == 0)
        {
            try
            {
                var ipData = await IpApiClient.GetIpData();
                Global.SensitiveData.Lat = ipData.Lat;
                Global.SensitiveData.Lon = ipData.Lon;
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "Failed getting geographic data");
            }
        }
    }

    private async Task DownloadAndExtract()
    {
        try
        {
            await WaitGithubAccess(TimeSpan.FromSeconds(60));
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Skipped Online Settings update because of internet problem");
            return;
        }

        DateTimeOffset commitDate;
        try
        {
            var settingsMeta = await OnlineConfigsRepository.GetOnlineSettingsOnline();
            commitDate = settingsMeta.OnlineSettingsTime;
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error fetching online settings");
            return;
        }

        var localSettings = await OnlineConfigsRepository.GetOnlineSettingsLocal();
        var localSettingsDate = localSettings.OnlineSettingsTime;

        if (commitDate <= localSettingsDate)
        {
            // no update required
            return;
        }

        Global.logger.Information("Updating Online Settings");

        try
        {
            await ExtractSettings();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error extracting online settings");
        }
    }

    private async Task Refresh()
    {
        try
        {
            await UpdateConflicts();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Failed to update conflicts");
        }

        try
        {
            await UpdateDeviceInfos();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Failed to update device infos");
        }

        try
        {
            await UpdateRazerMiceInfo();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Failed to update razer mice info");
        }
    }

    private async Task ExtractSettings()
    {
        const string zipUrl = "https://github.com/Aurora-RGB/Online-Settings/archive/refs/heads/master.zip";

        using var webClient = new WebClient();
        using var zipStream = new MemoryStream(webClient.DownloadData(zipUrl));
        await using var zipInputStream = new ZipInputStream(zipStream);
        while (zipInputStream.GetNextEntry() is { } entry)
        {
            if (!entry.IsFile)
                continue;

            var entryName = entry.Name;
            var fullPath = Path.Combine(".", entryName).Replace("\\Online-Settings-master", "");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await using var entryFileStream = File.Create(fullPath);
            await zipInputStream.CopyToAsync(entryFileStream);
        }
    }

    private async Task UpdateConflicts()
    {
        var conflictingProcesses = await OnlineConfigsRepository.GetConflictingProcesses();
        if (!Global.Configuration.EnableShutdownOnConflict || conflictingProcesses.ShutdownAurora == null)
        {
            return;
        }

        _shutdownProcesses = conflictingProcesses.ShutdownAurora.ToDictionary(p => p.ProcessName.ToLowerInvariant());
    }

    private async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason != SessionSwitchReason.SessionUnlock)
        {
            return;
        }

        await DownloadAndExtract();
        await Refresh();
    }

    private void OnRunningProcessesChanged(object? sender, ProcessStarted e)
    {
        if (!_shutdownProcesses.TryGetValue(e.ProcessName, out var shutdownProcess)) return;
        Global.logger.Fatal("Shutting down Aurora because of a conflicted process {Process}. Reason: {Reason}",
            shutdownProcess.ProcessName, shutdownProcess.Reason);
        App.ForceShutdownApp(-1);
    }

    private static async Task UpdateDeviceInfos()
    {
        DeviceTooltips = await OnlineConfigsRepository.GetDeviceTooltips();
    }

    private static async Task UpdateRazerMiceInfo()
    {
        RazerDeviceInfo = await OnlineConfigsRepository.GetRazerDeviceInfo();
    }

    async Task WaitGithubAccess(TimeSpan timeout)
    {
        using var cancelSource = new CancellationTokenSource();

        var resolveTask = WaitUntilResolve("github.com", cancelSource.Token);
        var delayTask = Task.Delay(timeout);

        var completedTask = await Task.WhenAny(resolveTask, delayTask);

        if (completedTask == delayTask)
        {
            await cancelSource.CancelAsync();
            throw new WebException("Failed to get github access");
        }
    }

    private async Task WaitUntilResolve(string domain, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var ips = await Dns.GetHostAddressesAsync(domain, cancellationToken);
                if (ips.Length > 0)
                {
                    return;
                }
            }
            catch
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        (await runningProcessMonitor).ProcessStarted -= OnRunningProcessesChanged;
    }
}