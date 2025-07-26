using System.Diagnostics;
using Serilog;

namespace Common.Utils;

public class PluginCompiler(ILogger logger, string path)
{
    public void Compile(string scriptPath)
    {
        var scriptChangeTime = File.GetLastWriteTime(scriptPath);
        var dllFile = scriptPath + ".dll";
        var dllCompileTime = File.Exists(dllFile) ? File.GetLastWriteTime(dllFile) : DateTime.UnixEpoch;

        if (scriptChangeTime < dllCompileTime)
        {
            logger.Information("[PluginCompiler] Script {Script} is up to date", scriptPath);
            return;
        }

        logger.Information("[PluginCompiler] Compiling script: {Script}", scriptPath);
        var compilerPath = Path.Combine(path, "PluginCompiler.exe");
        var compilerProc = new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(Environment.ProcessPath),
            FileName = compilerPath,
            Arguments = scriptPath,
        };
        var process = Process.Start(compilerProc);
        if (process == null)
        {
            throw new ApplicationException("PluginCompiler.exe not found!");
        }

        process.OutputDataReceived += (sender, e) =>
        {
            logger.Information("[PluginCompiler] Compiler: {Data}", e.Data);
        };
        process.ErrorDataReceived += (_, args) =>
        {
            logger.Error("[PluginCompiler] Compiler Error: {Data}", args.Data);
        };

        try
        {
            process.WaitForExit();
        }
        catch (Exception e)
        {
            logger.Error(e, "[PluginCompiler] Could not load script: {Script}", scriptPath);
            return;
        }
        logger.Information("[PluginCompiler] Script compiled successfully!");
    }
}