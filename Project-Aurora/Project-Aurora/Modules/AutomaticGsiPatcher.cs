using System;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Profiles;
using Application = AuroraRgb.Profiles.Application;

namespace AuroraRgb.Modules;

public sealed class AutomaticGsiPatcher : AuroraModule
{
    private const string InstallAutomaticallyPromptMessage = """
                                                             Would you like Aurora to install integrations automatically?

                                                             This includes game profiles that will be added later.

                                                             You can change this later in Aurora settings.
                                                             """;

    protected override async Task Initialize()
    {
        var userPromptTcs = new TaskCompletionSource<bool>();

        var lsm = await LightingStateManagerModule.LightningStateManager;

        lsm.ApplicationManager.EventAdded += (_, args) =>
        {
            var lightEvent = args.Application;
            Task.Run(async () =>
            {
                await RunInstallation(userPromptTcs, lightEvent);
            });
        };
        foreach (var application in lsm.ApplicationManager.Events.Values)
        {
            _ = Task.Run(async () =>
            {
                await RunInstallation(userPromptTcs, application);
            });
        }

        if (Global.Configuration.AutoInstallGsi != null)
        {
            userPromptTcs.SetResult(Global.Configuration.AutoInstallGsi ?? false);
            return;
        }

        var result = MessageBox.Show(InstallAutomaticallyPromptMessage, "AuroraRgb", MessageBoxButton.YesNo);
        Global.Configuration.AutoInstallGsi = result switch
        {
            MessageBoxResult.Yes => true,
            MessageBoxResult.No => false,
            _ => null,
        };
        userPromptTcs.SetResult(Global.Configuration.AutoInstallGsi ?? false);
    }

    private static async Task RunInstallation(TaskCompletionSource<bool> userPromptTcs, Application lightEvent)
    {
        try
        {
            var installGsi = await userPromptTcs.Task;
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