using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Aurora_Updater.Data;
using Octokit;
using Timer = System.Timers.Timer;
using Version = SemanticVersioning.Version;

namespace Aurora_Updater;

public class UpdateManager
{
    private const string DeviceManagerProcessName = "AuroraDeviceManager";
    private const string AuroraProcessName = "AuroraRgb";

    private readonly string[] _ignoreFiles = [];
    private readonly ObservableCollection<LogEntry> _log = [];
    private float _downloadProgress;
    private float _extractProgress;
    private int? _previousPercentage;
    private int _secondsLeft = 12;

    public readonly IReadOnlyList<Release> MissingReleases = [];
    public readonly Release? LatestRelease = new("Release fetch failed");
    private readonly LogEntry _downloadLogEntry = new("Download 0%");

    private readonly JsonSerializerOptions _jsonSerializerOptions = new(new JsonSerializerOptions
    {
        Converters = { new JsonStringEnumConverter() }
    });

    private readonly AuroraInterface _auroraInterface = new();

    public UpdateManager(Version version, string author, string repoName)
    {
        UpdaterConfiguration config;
        try
        {
            config = UpdaterConfiguration.Load();
        }
        catch
        {
            config = new UpdaterConfiguration(false);
        }

        var updateInfo = new UpdateInfo(version, author, repoName, config.GetDevReleases);
        var getPreReleases = updateInfo.IsCurrentlyPreRelease().Result;

        PerformCleanup();
        var tries = 6;
        while(tries-- != 0)
        {
            try
            {
                MissingReleases = updateInfo.FetchMissingReleases().ToList();
                LatestRelease = MissingReleases.FirstOrDefault(r => getPreReleases || !r.Prerelease);
                return;
            }
            catch (AggregateException e)
            {
                if (e.InnerException is HttpRequestException && tries != 0)
                {
                    Thread.Sleep(5000);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public void ClearLog()
    {
        _log.Clear();
    }

    public LogEntry[] GetLog()
    {
        return _log.ToArray();
    }

    public ObservableCollection<LogEntry> GetObservable()
    {
        return _log;
    }

    public int GetTotalProgress()
    {
        return (int)((_downloadProgress + _extractProgress) / 2.0f * 100.0f);
    }

    public async Task RetrieveUpdate()
    {
        if(LatestRelease == null)
            return;
        try
        {
            await SaveChangelogs(MissingReleases);

            var assets = LatestRelease.Assets;
            var url = assets.First(s => s.Name.StartsWith("release") || s.Name.StartsWith("Aurora-v")).BrowserDownloadUrl;

            if (string.IsNullOrWhiteSpace(url)) return;
            _log.Add(new LogEntry("Starting download... "));
            _log.Add(_downloadLogEntry);

            using var client = new WebClient();
            client.DownloadProgressChanged += client_DownloadProgressChanged;

            // Starts the download
            await client.DownloadFileTaskAsync(new Uri(url), Path.Combine(Program.ExePath, "update.zip"));

            var releaseAssets = assets.Where(a => a.Name.EndsWith(".dll")).ToList();
            if (releaseAssets.Count != 0)
            {
                await UpdatePlugin(releaseAssets, client);
            }

            _log.Add(new LogEntry("Download complete."));
            _log.Add(new LogEntry());
            _downloadProgress = 1.0f;

            if (ExtractUpdate())
            {
                _log.Add(new LogEntry("Update complete!\nThis window will self destruct in 10 seconds..."));
                var shutdownTimer = new Timer(1000);
                shutdownTimer.Elapsed += ShutdownTimerElapsed;
                shutdownTimer.Start();

                var auroraUpdated = MissingReleases.Any(r => ContainsComponentUpdate(r, UpdateComponent.Aurora));
                var deviceManagerUpdated = MissingReleases.Any(r => ContainsComponentUpdate(r, UpdateComponent.DeviceManager));

                if (!auroraUpdated && deviceManagerUpdated)
                {
                    await RestartDeviceManager();
                }
                else
                {
                    await (deviceManagerUpdated ? ShutdownDeviceManager() : Task.CompletedTask);
                    await (auroraUpdated ? RestartAurora() : Task.CompletedTask);
                }

                PerformCleanup();
            }
        }
        catch (Exception exc)
        {
            _log.Add(new LogEntry(exc.Message, Color.Red));
        }

        bool ContainsComponentUpdate(Release release, UpdateComponent component)
        {
            try
            {
                var componentsLine = release.Body[release.Body.LastIndexOf('\n')..];
                var updateComponents = JsonSerializer.Deserialize<UpdateComponent[]>(componentsLine, _jsonSerializerOptions)!;
                return updateComponents.Contains(component);
            }
            catch
            {
                return false;
            }
        }
    }

    private static async Task SaveChangelogs(IEnumerable<Release> releases)
    {
        foreach (var release in releases)
        {
            await SaveChangelog(release);
        }
    }

    private static async Task SaveChangelog(Release release)
    {
        var changelogFile = $"./changelogs/{release.TagName}.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(changelogFile)!);

        if (release.Body.EndsWith(']'))
        {
            var changelog = release.Body[..release.Body.LastIndexOf('\n')];
            await File.WriteAllTextAsync(changelogFile, changelog);
        }
        else
        {
            await File.WriteAllTextAsync(changelogFile, release.Body);
        }
    }

    private async Task UpdatePlugin(IEnumerable<ReleaseAsset> releaseAssets, WebClient client)
    {
        var installDirPlugin = Path.Combine(Program.ExePath, "Plugins");
        var userDirPlugin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Plugins");

        var deviceInstallDirPlugin = Path.Combine(installDirPlugin, "Devices");
        var deviceUserDirPlugin = Path.Combine(userDirPlugin, "Devices");

        var pluginUpdaters = from pluginDll in releaseAssets
            let address = new Uri(pluginDll.BrowserDownloadUrl)
            select new PluginUpdater(pluginDll, client, address, _log);
        foreach (var pluginUpdater in pluginUpdaters)
        {
            await pluginUpdater.UpdatePlugin(installDirPlugin);
            await pluginUpdater.UpdatePlugin(userDirPlugin);
            await pluginUpdater.UpdatePlugin(deviceInstallDirPlugin);
            await pluginUpdater.UpdatePlugin(deviceUserDirPlugin);
        }
    }

    private sealed class PluginUpdater(ReleaseAsset pluginDll, WebClient client, Uri address, ICollection<LogEntry> log)
    {
        internal async Task UpdatePlugin(string installDirPlugin)
        {
            if (File.Exists(installDirPlugin))
            {
                log.Add(new LogEntry("Updating " + pluginDll.Name));
                await client.DownloadFileTaskAsync(address, installDirPlugin);
            }
        }
    }

    private void client_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        var bytesIn = e.BytesReceived;
        var totalBytes = e.TotalBytesToReceive;
        var percentage = (double)bytesIn / totalBytes;

        var newPercentage = (int)(percentage * 100);
        if (_previousPercentage == newPercentage)
            return;

        _previousPercentage = newPercentage;

        _downloadLogEntry.Message = $"Download {newPercentage}%";
        _downloadProgress = newPercentage / 100.0f;
    }

    private void ShutdownTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_secondsLeft > 0)
        {
            _secondsLeft--;
            return;
        }

        Environment.Exit(0); //Exit, no further action required
    }

    private async Task RestartAurora()
    {
        //gracefully, find currently open aurora processes
        var auroraProcesses = Process.GetProcessesByName(AuroraProcessName);
        var auroraExitTasks = auroraProcesses
            .Select(p => p.WaitForExitAsync());

        try
        {
            if (auroraProcesses.Length > 0)
            {
                try
                {
                    await _auroraInterface.RestartAurora();
                }
                catch
                {
                    //ignore
                }
            }

            //Kill all Aurora instances
            await Task.WhenAny(
                Task.Delay(TimeSpan.FromSeconds(10)),
                Task.WhenAll(auroraExitTasks)
            );

            //forcefully
            foreach (var proc in auroraProcesses)
                proc.Kill();

            if (auroraProcesses.Length == 0)
            {
                StartAurora();
            }
        }
        catch (Exception exc)
        {
            _log.Add(new LogEntry($"Could not restart Aurora. Error:\r\n{exc}", Color.Red));
            _log.Add(new LogEntry("Please restart Aurora manually.", Color.Red));

            MessageBox.Show(
                $"Could not restart Aurora.\r\nPlease restart Aurora manually.\r\nError:\r\n{exc}",
                "Aurora Updater - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task RestartDeviceManager()
    {
        await _auroraInterface.RestartDeviceManager();
    }

    private async Task<bool> ShutdownDeviceManager()
    {
        var dmProcesses = Process.GetProcessesByName(DeviceManagerProcessName);
        var dmExitTasks = dmProcesses
            .Select(p => p.WaitForExitAsync());
        if (dmProcesses.Length <= 0)
        {
            return false;
        }

        try
        {
            await _auroraInterface.ShutdownDeviceManager();
            //Kill all Aurora instances
            await Task.WhenAny(
                Task.Delay(TimeSpan.FromSeconds(5)),
                Task.WhenAll(dmExitTasks)
            );

            //forcefully
            foreach (var proc in dmProcesses)
                proc.Kill();
        }
        catch
        {
            //ignore    
        }
        return true;
    }

    private void StartAurora()
    {
        var auroraProc = new ProcessStartInfo
        {
            FileName = Path.Combine(Program.ExePath, "AuroraRgb.exe")
        };
        Process.Start(auroraProc);
    }

    private bool ExtractUpdate()
    {
        if (!File.Exists(Path.Combine(Program.ExePath, "update.zip")))
        {
            _log.Add(new LogEntry("Update file not found.", Color.Red));
            return false;
        }

        _log.Add(new LogEntry("Unpacking update..."));

        try
        {
            var updateFile = ZipFile.OpenRead(Path.Combine(Program.ExePath, "update.zip"));
            var countOfEntries = updateFile.Entries.Count;
            _log.Add(new LogEntry($"{countOfEntries} files detected."));

            for (var i = 0; i < countOfEntries; i++)
            {
                var percentage = i / (float)countOfEntries;

                var fileEntry = updateFile.Entries[i];
                _log.Add(new LogEntry($"[{Math.Truncate(percentage * 100)}%] Updating: {fileEntry.FullName}"));
                _extractProgress = (float)(Math.Truncate(percentage * 100) / 100.0f);

                if (Path.EndsInDirectorySeparator(fileEntry.FullName))
                    continue;

                if (_ignoreFiles.Contains(fileEntry.FullName))
                    continue;

                try
                {
                    var filePath = Path.Combine(Program.ExePath, fileEntry.FullName);
                    if (File.Exists(filePath))
                        File.Move(filePath, $"{filePath}.updateremove");
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    fileEntry.ExtractToFile(filePath, true);
                }
                catch (IOException e)
                {
                    if (fileEntry.FullName.Contains("/AuroraDeviceManager/"))
                    {
                        var processClosed = ShutdownDeviceManager().Result;
                        if (!processClosed)
                        {
                            MessageBox.Show($"{fileEntry.FullName} is inaccessible.\r\nPlease close AuroraRgb.exe and AuroraDeviceManager.exe.\n\n {e.StackTrace}");
                        }
                        i--;
                        continue;
                    }

                    var processes = Process.GetProcessesByName(AuroraProcessName);
                    foreach (var auroraProcess in processes)
                    {
                        try
                        {
                            auroraProcess.Kill();
                        }catch { /* probably closed anyway */ }
                    }

                    //automatically retry without notifying user
                    if (processes.Length > 0)
                    {
                        i--;
                        continue;
                    }
                    _log.Add(new LogEntry($"{fileEntry.FullName} is inaccessible.", Color.Red));

                    MessageBox.Show($"{fileEntry.FullName} is inaccessible.\r\nPlease close AuroraRgb.exe and AuroraDeviceManager.exe.\n\n {e.StackTrace}");
                    i--;
                }
            }

            updateFile.Dispose();
            File.Delete(Path.Combine(Program.ExePath, "update.zip"));
        }
        catch (Exception exc)
        {
            _log.Add(new LogEntry(exc.Message, Color.Red));

            return false;
        }

        _log.Add(new LogEntry("All files updated."));
        _log.Add(new LogEntry());
        _log.Add(new LogEntry("Updater will automatically restart Aurora."));
        _extractProgress = 1.0f;

        return true;
    }

    private void PerformCleanup()
    {
        var messyFiles = Directory.GetFiles(Program.ExePath, "*.updateremove", SearchOption.AllDirectories);

        foreach (var file in messyFiles)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                _log.Add(new LogEntry("Unable to delete file - " + file, Color.Red));
            }
        }

        if (File.Exists(Path.Combine(Program.ExePath, "update.zip")))
            File.Delete(Path.Combine(Program.ExePath, "update.zip"));
    }
}