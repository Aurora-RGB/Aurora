using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace AuroraRgb.Modules.Logitech;

public static class LgsInstallationUtils
{
    public const string LgsExe = "lcore.exe";

    public static bool IsLgsInstalled()
    {
        const string runReg = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        using var runKey = Registry.LocalMachine.OpenSubKey(runReg);
        var lgsLaunch = runKey?.GetValue("Launch LCore");
        var lgsInstalled = lgsLaunch != null;
        return lgsInstalled;
    }

    public static bool LgsAutorunEnabled()
    {
        const string runApprovedReg = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        using var runApprovedKey = Registry.LocalMachine.OpenSubKey(runApprovedReg);
        var lgsLaunchApproved = runApprovedKey?.GetValue("Launch LCore");
        var runApproved = lgsLaunchApproved is byte[] startValue && startValue[0] == 2;
        return runApproved;
    }

    public static bool DllInstalled()
    {
        const string registryPath64 = @"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary";
        const string registryPath32 = @"SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary";

        using var key64 = Registry.LocalMachine.OpenSubKey(registryPath64);
        using var key32 = Registry.LocalMachine.OpenSubKey(registryPath32);

        var file64 = key64?.GetValue(null)?.ToString();
        var file32 = key32?.GetValue(null)?.ToString();

        return DllInstalled(file64) && DllInstalled(file32);
    }

    private static bool DllInstalled(string? path)
    {
        const string dllProductName = "Logitech Gaming LED SDK";

        if (path == null)
        {
            return false;
        }

        var present = File.Exists(path);
        if (!present)
        {
            return false;
        }

        var attributes = FileVersionInfo.GetVersionInfo(path);
        return attributes.FileDescription == dllProductName;
    }
}