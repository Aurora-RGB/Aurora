using System;
using System.Threading.Tasks;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Profiles;
using RazerSdkReader;

namespace AuroraRgb.Modules;

public sealed class RazerSdkModule(Task<LightingStateManager> lsm) : AuroraModule
{
    private readonly TaskCompletionSource<ChromaReader?> _sdkTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private ChromaReader? _razerSdkManager;

    public Task<ChromaReader?> RzSdkManager => _sdkTaskSource.Task;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading RazerSdkManager");
        if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
        {
            try
            {
                await lsm; //wait for ChromaApplication.Settings to load
                TryLoadChroma();
                if (Global.Configuration.ChromaDisableDeviceControl)
                {
                    try
                    {
                        await RazerChromaUtils.DisableDeviceControlAsync();
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error(e, "Error disabling device control automatically");
                    }
                }

                Global.logger.Information("RazerSdkManager loaded successfully!");
                _sdkTaskSource.SetResult(_razerSdkManager);
            }
            catch (Exception exc)
            {
                Global.logger.Fatal(exc, "RazerSdkManager failed to load!");
                _sdkTaskSource.SetResult(null);
            }
        }
        else
        {
            Global.logger.Warning("Currently installed razer sdk version \"{RzVersion}\" is not supported by the RazerSdkManager!",
                RzHelper.GetSdkVersion());
            _sdkTaskSource.SetResult(null);
        }
    }

    private void TryLoadChroma()
    {
        _razerSdkManager = new ChromaReader();
        _razerSdkManager.Exception += RazerSdkManagerOnException;
        Global.razerSdkManager = _razerSdkManager;
        RzHelper.Initialize();

        _razerSdkManager.Start();
    }

    private void RazerSdkManagerOnException(object? sender, RazerSdkReaderException e)
    {
        Global.logger.Error(e, "Chroma Reader Error");
    }

    public override ValueTask DisposeAsync()
    {
        try
        {
            if (_razerSdkManager == null)
            {
                return ValueTask.CompletedTask;
            }
            _razerSdkManager.Dispose();
            Global.razerSdkManager = null;
        }
        catch (Exception exc)
        {
            Global.logger.Fatal(exc, "RazerManager failed to dispose!");
        }
        return ValueTask.CompletedTask;
    }
}