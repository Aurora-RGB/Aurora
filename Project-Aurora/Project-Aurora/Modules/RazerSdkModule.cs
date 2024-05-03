using System;
using System.Threading.Tasks;
using AuroraRgb.Modules.Razer;

namespace AuroraRgb.Modules;

public sealed class RazerSdkModule(Task lsmLoadTask) : AuroraModule
{
    private readonly ChromaSdkManager _razerSdkManager = new();

    public Task<ChromaSdkManager> RzSdkManager => Task.FromResult(_razerSdkManager);

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

        await lsmLoadTask; //wait for ChromaApplication.Settings to load TODO decouple chroma settings from profile
        await _razerSdkManager.Initialize();
        Global.logger.Information("RazerSdkManager loaded successfully!");
    }

    public override ValueTask DisposeAsync()
    {
        try
        {
            _razerSdkManager.Dispose();
        }
        catch (Exception exc)
        {
            Global.logger.Fatal(exc, "RazerManager failed to dispose!");
        }
        return ValueTask.CompletedTask;
    }
}