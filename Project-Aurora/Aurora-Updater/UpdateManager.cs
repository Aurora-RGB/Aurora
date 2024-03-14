using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    private readonly string[] _ignoreFiles = [];
    private readonly ObservableCollection<LogEntry> _log = [];
    private float _downloadProgress;
    private float _extractProgress;
    private int? _previousPercentage;
    private int _secondsLeft = 3;

    public readonly Release LatestRelease;
    private readonly LogEntry _downloadLogEntry = new("Download 0%");

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

        PerformCleanup();
        var tries = 20;
        do
        {
            try
            {
                LatestRelease = updateInfo.FetchData().Result;
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
        } while (LatestRelease == null && tries-- != 0);

        LatestRelease = new Release("Release fetch failed");
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
        try
        {
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
                var installDirPlugin = Path.Combine(Program.ExePath, "Plugins");
                var userDirPlugin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Plugins");

                var deviceInstallDirPlugin = Path.Combine(installDirPlugin, "Devices");
                var deviceUserDirPlugin = Path.Combine(userDirPlugin, "Devices");

                foreach (var pluginDll in releaseAssets)
                {
                    var address = new Uri(pluginDll.BrowserDownloadUrl);
                    var pluginUpdater = new PluginUpdater(pluginDll, client, address, _log);

                    await pluginUpdater.UpdatePlugin(installDirPlugin);
                    await pluginUpdater.UpdatePlugin(userDirPlugin);
                    await pluginUpdater.UpdatePlugin(deviceInstallDirPlugin);
                    await pluginUpdater.UpdatePlugin(deviceUserDirPlugin);
                }
            }

            _log.Add(new LogEntry("Download complete."));
            _log.Add(new LogEntry());
            _downloadProgress = 1.0f;

            if (ExtractUpdate())
            {
                var shutdownTimer = new Timer(1000);
                shutdownTimer.Elapsed += ShutdownTimerElapsed;
                shutdownTimer.Start();
            }
        }
        catch (Exception exc)
        {
            _log.Add(new LogEntry(exc.Message, Color.Red));
        }
    }

    private class PluginUpdater
    {
        private readonly ObservableCollection<LogEntry> _log;
        private readonly ReleaseAsset _pluginDll;
        private readonly WebClient _client;
        private readonly Uri _address;

        public PluginUpdater(ReleaseAsset pluginDll, WebClient client, Uri address, ObservableCollection<LogEntry> log)
        {
            _pluginDll = pluginDll;
            _client = client;
            _address = address;
            _log = log;
        }

        internal async Task UpdatePlugin(string installDirPlugin)
        {
            if (File.Exists(installDirPlugin))
            {
                _log.Add(new LogEntry("Updating " + _pluginDll.Name));
                await _client.DownloadFileTaskAsync(_address, installDirPlugin);
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
            _log.Add(new LogEntry($"Restarting Aurora in {_secondsLeft} second{(_secondsLeft == 1 ? "" : "s")}..."));
            _secondsLeft--;
        }
        else
        {
            //Kill all Aurora instances
            var auroraInterface = new AuroraInterface();
            auroraInterface.ShutdownAurora().Wait();

            try
            {
                var auroraProc = new ProcessStartInfo
                {
                    FileName = Path.Combine(Program.ExePath, "AuroraRgb.exe")
                };
                Process.Start(auroraProc);

                Environment.Exit(0); //Exit, no further action required
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
    }

    private bool ExtractUpdate()
    {
        if (File.Exists(Path.Combine(Program.ExePath, "update.zip")))
        {
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
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        fileEntry.ExtractToFile(filePath, true);
                    }
                    catch (IOException e)
                    {
                        _log.Add(new LogEntry($"{fileEntry.FullName} is inaccessible.", Color.Red));

                        MessageBox.Show($"{fileEntry.FullName} is inaccessible.\r\nPlease close Aurora.\r\n\r\n {e.StackTrace}");
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

        _log.Add(new LogEntry("Update file not found.", Color.Red));
        return false;
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

public enum UpdateType
{
    Undefined,
    Major,
    Minor
}