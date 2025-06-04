using System.Collections.Frozen;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace AuroraRgb.Modules.Logitech;

public static class LgsInstallationUtils
{
    public const string LgsExe = "lcore.exe";
    private static readonly FrozenSet<string> LgsProductNames = new[]
            {
                "Logitech Gaming LED SDK",
            }.ToFrozenSet();

    private static readonly FrozenSet<string> LgsAutoStartNames = new[]
    {
        "Launch LCore",
        "LCore.exe",
    }.ToFrozenSet();

    public static bool IsLgsInstalled()
    {
        const string runReg = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        using var runKey = Registry.LocalMachine.OpenSubKey(runReg);
        
        if (runKey == null)
            return false;
        return LgsAutoStartNames.Any(IsLgsRegistry);

        bool IsLgsRegistry(string keyValue)
        {
            var lgsLaunch = runKey.GetValue(keyValue);
            var lgsInstalled = lgsLaunch != null;
            return lgsInstalled;
        }
    }

    public static bool LgsAutorunEnabled()
    {
        const string runApprovedReg = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        using var runApprovedKey = Registry.LocalMachine.OpenSubKey(runApprovedReg);
        
        if (runApprovedKey == null)
            return false;
        return LgsAutoStartNames.Any(IsLgsRunEnabled);
        
        bool IsLgsRunEnabled(string keyValue)
        {
            var lgsLaunch = runApprovedKey.GetValue(keyValue);
            var valueNull = lgsLaunch != null;
            if (!valueNull)
            {
                return false;
            }

            if (lgsLaunch is not byte[] valueBytes || valueBytes.Length < 1)
            {
                return false;
            }
            var enabledFlag = valueBytes[0];
            return enabledFlag == 2;
        }
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
        var productName = attributes.FileDescription;
        return productName != null && LgsProductNames.Contains(productName);
    }
}