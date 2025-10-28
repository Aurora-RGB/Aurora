using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Version = SemanticVersioning.Version;

namespace Aurora_Updater;

internal static class Program
{
    private static string _passedArgs = "";
    private static bool _isSilent;
    public static string ExePath { get; private set; } = "";
    private static UpdateType _installType = UpdateType.Undefined;
    private static bool _isElevated;
    private static bool _isBackground;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        using Mutex mutex = new(false, "Aurora-Updater");
        try
        {
            if (!mutex.WaitOne(TimeSpan.FromMilliseconds(0), true))
            {
                //Updater is already up
                return;
            }
        }
        catch(AbandonedMutexException) { /* Means previous instance closed anyway */ }

        ProcessArgs(args);

        if (!GetExePath(out var exePath))
        {
            if (!_isSilent)
                MessageBox.Show(
                    "Cannot determine installation location",
                    "Aurora Updater",
                    MessageBoxButtons.OK);
            return;
        }
        ExePath = exePath;

        //Check privilege
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        _isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
        var fileVersion = "";

        var auroraPath = Path.Combine(ExePath, "AuroraRgb.exe");
        if (File.Exists(auroraPath))
            fileVersion = FileVersionInfo.GetVersionInfo(auroraPath).FileVersion;

        if (string.IsNullOrWhiteSpace(fileVersion))
        {
            if (!_isSilent)
                MessageBox.Show(
                    "Application launched incorrectly, no version was specified.\r\n" +
                    "Please use Aurora if you want to check for updates.\r\n" +
                    "Options -> \"Updates\" \"Check for Updates\"",
                    "Aurora Updater",
                    MessageBoxButtons.OK);
            return;
        }
        var versionToCheck = VersionParser.ParseVersion(fileVersion);
        if (versionToCheck is { Major: 0, Minor: 0, Patch: 0 })
        {
            _isSilent = false;
        }

        var owner = FileVersionInfo.GetVersionInfo(auroraPath).CompanyName;
        var repository = FileVersionInfo.GetVersionInfo(auroraPath).ProductName;
        if (owner == null || repository == null)
        {
            MessageBox.Show(
                "Exe owner/repository is not set. This is needed for checking releases on Github",
                "Aurora Updater - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        //Initialize UpdateManager
        if(!TryGetUpdateManager(versionToCheck, owner, repository, out var updateManager))
        {
            return;
        }
        if(updateManager.LatestRelease == null)
            return;

        var latestV = VersionParser.ParseVersion(updateManager.LatestRelease.TagName);
        if (File.Exists("skipversion.txt"))
        {
            var skippedVersion = VersionParser.ParseVersion(File.ReadAllText("skipversion.txt"));
            if (skippedVersion >= latestV)
            {
                return;
            }
        }

        if (latestV <= versionToCheck)
        {
            if (!_isSilent)
            {
                MessageBox.Show(
                    "You have latest version of Aurora installed.",
                    "Aurora Updater",
                    MessageBoxButtons.OK);
            }
            return;
        }

        var latestReleaseAssets = updateManager.LatestRelease.Assets;
        var releaseAsset = latestReleaseAssets
            .First(s => s.Name.StartsWith("release") || s.Name.StartsWith("Aurora-v"));
        var installerAsset = latestReleaseAssets.First(s => s.Name.StartsWith("Aurora-setup-v"));
        var userResult = new UpdateInfoForm
        {
            Changelog = updateManager.LatestRelease.Body,
            UpdateDescription = updateManager.LatestRelease.Name,
            UpdateVersion = latestV.ToString(),
            CurrentVersion = versionToCheck.ToString(),
            UpdateSize = releaseAsset.Size,
            PreRelease = updateManager.LatestRelease.Prerelease,
            UpdateTime = releaseAsset.CreatedAt,
            DownloadTime = releaseAsset.DownloadCount + installerAsset?.DownloadCount ?? 0,
        };
        
        if (!_isElevated)
        {
            //Request user to grant admin rights
            TryRunAsAdmin();
            return;
        }

        if (updateManager.UpdateInfo.IsDevelopmentBuild())
        {
            _isBackground = false;
        }

        var updaterThread = new Thread(() => updateManager.RetrieveUpdate().Wait())
        {
            IsBackground = !_isBackground,
        };

        if (_isBackground)
        {
            updaterThread.Start();
            return;
        }

        userResult.ShowDialog();
        if (userResult.DialogResult != DialogResult.OK)
            return;

        var updateForm = new MainForm(updateManager);
        updaterThread.Start();
        updateForm.ShowDialog();
    }
    
    private static bool TryGetUpdateManager(Version version, string owner, string repository, [MaybeNullWhen(false)] out UpdateManager updateManager)
    {
        try
        {
            updateManager = new UpdateManager(version, owner, repository);
            return true;
        }
        catch (Exception exc)
        {
            MessageBox.Show(
                $"Could not find update.\r\nError:\r\n{exc}",
                "Aurora Updater - Error");
        }

        updateManager = null;
        return false;
    }

    private static void ProcessArgs(IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (string.IsNullOrWhiteSpace(arg))
                continue;

            switch (arg)
            {
                case "-silent":
                    _isSilent = true;
                    break;
                case "-update_major":
                    _installType = UpdateType.Major;
                    break;
                case "-update_minor":
                    _installType = UpdateType.Minor;
                    break;
                case "-background":
                    _isBackground = true;
                    break;
            }

            _passedArgs += arg + " ";
        }

        _passedArgs = _passedArgs.TrimEnd(' ');
    }

    private static bool GetExePath([MaybeNullWhen(false)] out string exePath)
    {
        exePath = Path.GetDirectoryName(Environment.ProcessPath);
        return exePath != null;
    }

    private static void TryRunAsAdmin()
    {
        try
        {
            var updaterProc = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                Arguments = _passedArgs + " -update_major",
                Verb = "runas"
            };
            Process.Start(updaterProc);
        }
        catch (Exception exc)
        {
            MessageBox.Show(
                $"Could not start Aurora Updater. Error:\r\n{exc}",
                "Aurora Updater - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}