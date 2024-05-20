using System;
using System.Threading.Tasks;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Settings;

namespace AuroraRgb.Modules;

public sealed class RazerSdkModule : AuroraModule
{

    private static readonly TaskCompletionSource<ChromaSdkManager> RzSdkManagerTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static Task<ChromaSdkManager> RzSdkManager => RzSdkManagerTaskSource.Task;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading RazerSdkManager");
        if (!RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
        {
            Global.logger.Warning("Currently installed razer sdk version \"{RzVersion}\" is not supported by the RazerSdkManager!", RzHelper.GetSdkVersion());
            return;
        }

        if (Global.Configuration.ChromaDisableDeviceControl)
        {
            try
            {
                await ChromaInstallationUtils.DisableDeviceControlAsync();
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "Error disabling device control automatically");
            }
        }
        
        var auroraChromaSettings = await ConfigManager.LoadChromaConfig();
        var razerSdkManager = new ChromaSdkManager(auroraChromaSettings);
        await razerSdkManager.Initialize();
        RzSdkManagerTaskSource.SetResult(razerSdkManager);
        Global.logger.Information("RazerSdkManager loaded successfully!");
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            if (!RzSdkManager.IsCompleted)
            {
                return;
            }
            (await RzSdkManager).Dispose();
        }
        catch (Exception exc)
        {
            Global.logger.Fatal(exc, "RazerManager failed to dispose!");
        }
    }
}