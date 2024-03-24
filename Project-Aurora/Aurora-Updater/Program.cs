using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using System.Linq;
using System.Threading;

namespace Aurora_Updater;

/// <summary>
///     The static storage.
/// </summary>
public static class StaticStorage
{
    #region Static Fields

    /// <summary>
    ///     The index.
    /// </summary>
    public static UpdateManager Manager;

    #endregion
}

internal static class Program
{
    private static string _passedArgs = "";
    private static bool _isSilent;
    public static string ExePath { get; private set; } = "";
    private static UpdateType _installType = UpdateType.Undefined;
    private static bool _isElevated;

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
        var versionMajor = VersionParser.ParseVersion(fileVersion);

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
        try
        {
            StaticStorage.Manager = new UpdateManager(versionMajor, owner, repository);
        }
        catch (Exception exc)
        {
            MessageBox.Show(
                $"Could not find update.\r\nError:\r\n{exc}",
                "Aurora Updater - Error");
            return;
        }

        if (_installType != UpdateType.Undefined)
        {
            if (_isElevated)
            {
                var updateForm = new MainForm();
                updateForm.ShowDialog();
            }
            else
            {
                MessageBox.Show(
                    "Updater was not granted Admin rights.",
                    "Aurora Updater - Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        else
        {
            var latestV = VersionParser.ParseVersion(StaticStorage.Manager.LatestRelease.TagName);
            if (File.Exists("skipversion.txt"))
            {
                var skippedVersion = VersionParser.ParseVersion(File.ReadAllText("skipversion.txt"));
                if (skippedVersion >= latestV)
                {
                    return;
                }
            }

            if (latestV <= versionMajor)
            {
                if (!_isSilent)
                    MessageBox.Show(
                        "You have latest version of Aurora installed.",
                        "Aurora Updater",
                        MessageBoxButtons.OK);
            }
            else
            {
                var latestReleaseAssets = StaticStorage.Manager.LatestRelease.Assets;
                var releaseAsset = latestReleaseAssets
                    .First(s => s.Name.StartsWith("release") || s.Name.StartsWith("Aurora-v"));
                var installerAsset = latestReleaseAssets.First(s => s.Name.StartsWith("Aurora-setup-v"));
                var userResult = new UpdateInfoForm
                {
                    Changelog = StaticStorage.Manager.LatestRelease.Body,
                    UpdateDescription = StaticStorage.Manager.LatestRelease.Name,
                    UpdateVersion = latestV.ToString(),
                    CurrentVersion = versionMajor.ToString(),
                    UpdateSize = releaseAsset.Size,
                    PreRelease = StaticStorage.Manager.LatestRelease.Prerelease,
                    UpdateTime = releaseAsset.CreatedAt,
                    DownloadTime = releaseAsset.DownloadCount + installerAsset?.DownloadCount ?? 0,
                };

                userResult.ShowDialog();

                if (userResult.DialogResult != DialogResult.OK) return;
                if (_isElevated)
                {
                    var updateForm = new MainForm();
                    updateForm.ShowDialog();
                }
                else
                {
                    //Request user to grant admin rights
                    TryRunAsAdmin();
                }
            }
        }
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