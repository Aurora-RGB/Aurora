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

        var is64BitKeyPresent = File.Exists(key64?.GetValue(null)?.ToString()) ;
        var is32BitKeyPresent = File.Exists(key32?.GetValue(null)?.ToString());
        
        return is64BitKeyPresent && is32BitKeyPresent;
    }
}