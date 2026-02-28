using System;
using System.Threading.Tasks;
using AuroraRgb.Profiles;
using Application = AuroraRgb.Profiles.Application;

namespace AuroraRgb.Modules;

public sealed class AutomaticGsiPatcher(Task<bool> autoGsiSettingTask) : AuroraModule
{
    protected override async Task Initialize()
    {
        var lsm = await LightingStateManagerModule.LightningStateManager;
        var autoInstallGsi = await autoGsiSettingTask;

        lsm.ApplicationManager.EventAdded += (_, args) =>
        {
            var lightEvent = args.Application;
            Task.Run(async () =>
            {
                await RunInstallation(autoInstallGsi, lightEvent);
            });
        };
        foreach (var application in lsm.ApplicationManager.Events.Values)
        {
            _ = Task.Run(async () =>
            {
                await RunInstallation(autoInstallGsi, application);
            });
        }
    }

    private static async Task RunInstallation(bool installGsi, Application lightEvent)
    {
        try
        {
            if (!installGsi)
            {
                return;
            }

            await InstallAppGsi(lightEvent);
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "[AutomaticGsiPatcher] An error occured while installing Gsi of {App}", lightEvent.Config.Name);
        }
    }

    private static async Task InstallAppGsi(Application lightEvent)
    {
        switch (lightEvent)
        {
            case GsiApplication application:
                var retries = 5;
                while (retries-- > 0)
                {
                    if (application.Settings != null)
                    {
                        if (!application.Settings.InstallationCompleted)
                        {
                            Global.logger.Information("[AutomaticGsiPatcher] Installing {App} Gsi", application.Config.Name);
                            await application.InstallGsi();
                            await application.SaveSettings();
                            Global.logger.Information("[AutomaticGsiPatcher] {App} Gsi installed", application.Config.Name);
                        }
                    }
                    else
                    {
                        Global.logger.Error("[AutomaticGsiPatcher] {App} settings not loaded to determine GSI installation status", application.Config.Name);
                        await Task.Delay(200);
                        continue;
                    }

                    break;
                }

                break;
        }
    }

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}