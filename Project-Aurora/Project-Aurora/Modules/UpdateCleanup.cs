using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuroraRgb.Utils;

namespace AuroraRgb.Modules;

public sealed partial class UpdateCleanup : AuroraModule
{
    private const string Net10MigrationKey = "net10v1";

    protected override Task Initialize()
    {
        AutoStartUtils.GetAutostartTask(out _);
        if (!Global.Configuration.Migrations.Contains(Net10MigrationKey))
        {
            Net10V1Migration();
            Global.Configuration.Migrations.Add(Net10MigrationKey);
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

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private static void Net10V1Migration()
    {
        // read net10_extra_files.txt from Resources and delete any files that exist in the list
        var resource = Properties.Resources.net10_extra_files;
        
        var files = from line in Encoding.UTF8.GetString(resource)
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            let file = line.Trim()
            where !string.IsNullOrEmpty(file)
            select Path.Combine(Global.ExecutingDirectory, file);

        foreach (var file in files.Where(File.Exists))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "[UpdateCleanup] Failed to delete file: {File}", file);
            }
        }
    }

    [GeneratedRegex(".*\\.log")]
    private static partial Regex LogFileRegex();
}