using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuroraRgb.Utils;
using Lombok.NET;

namespace AuroraRgb.Modules;

public sealed partial class UpdateCleanup : AuroraModule
{
    protected override Task Initialize()
    {
        AutoStartUtils.GetAutostartTask(out _);
        if (!Global.Configuration.Migrations.Contains("net8v3"))
        {
            Net8V2Migration();
            Global.Configuration.Migrations.Add("net8v3");
        }
        CleanOldLogiDll();
        CleanLogs();
        CleanOldExe();
        return Task.CompletedTask;
    }

    private static void CleanOldLogiDll()
    {
        var logiDll = Path.Combine(Global.ExecutingDirectory, "LogitechLed.dll");
        if (File.Exists(logiDll))
            File.Delete(logiDll);
    }

    private static void CleanLogs()
    {
        var logFolder = Path.Combine(Global.AppDataDirectory, "Logs");

        var logFile = LogFileRegex();
        var files = from file in Directory.EnumerateFiles(logFolder)
            where logFile.IsMatch(Path.GetFileName(file))
            orderby File.GetCreationTime(file) descending
            select file;
        foreach (var file in files.Skip(8))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "Failed to delete log: {File}", file);
            }
        }
    }

    private static void CleanOldExe()
    {
        var oldExe = Path.Combine(Global.ExecutingDirectory, "Aurora.exe");
        if (File.Exists(oldExe))
        {
            File.Delete(oldExe);
        }
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }

    private void Net8V2Migration()
    {
        IEnumerable<string> files = [
            "Bloody.NET.dll",
            "clrcompression.dll",
            "CTIntrfu.dll",
            "DS4WindowsApi.dll",
            "HidLibrary.dll",
            "mscordaccore_amd64_amd64_6.0.2423.51814.dll",
            "OmenFourZoneLighting.dll",
            "OpenRGB.NET.dll",
            "RGB.NET.Devices.Asus.dll",
            "RGB.NET.Devices.Bloody.dll",
            "RGB.NET.Devices.Bloody.pdb",
            "RGB.NET.Devices.CoolerMaster.dll",
            "RGB.NET.Devices.OpenRGB.dll",
            "RGB.NET.Devices.Razer.dll",
            "RGB.NET.Devices.SteelSeries.dll",
            "RGB.NET.Devices.Wooting.dll",
            "RGB.NET.YeeLightStates.dll",
            "RGB.NET.YeeLightStates.pdb",
            "Roccat-Talk.dll",
            "SBAuroraReactive.dll",
            "UniwillSDKDLL.dll",
            "Vulcan.NET.dll",
            "wooting-rgb-sdk.dll",
            "Wooting.NET.dll",
            "YeeLightAPI.dll",
            "mscordaccore_amd64_amd64_6.0.2623.60508.dll",
            "mscordaccore_amd64_amd64_6.0.2724.6912.dll",
            "wpfgfx_cor3.dll",
        ];

        foreach (var file in files.Where(File.Exists))
        {
            File.Delete(file);
        }
    }

    [GeneratedRegex(".*\\.log")]
    private static partial Regex LogFileRegex();
}