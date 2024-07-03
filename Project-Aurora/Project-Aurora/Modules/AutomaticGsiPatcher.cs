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

        lsm.EventAdded += (_, args) =>
        {
            var lightEvent = args.Application;
            Task.Run(async () =>
            {
                var installGsi = await userPromptTcs.Task;
                if (!installGsi)
                {
                    return;
                }
                await InstallAppGsi(lightEvent);
            });
        };

        if (Global.Configuration.AutoInstallGsi != null)
        {
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

    private static async Task InstallAppGsi(Application lightEvent)
    {
        switch (lightEvent)
        {
            case GsiApplication application:
                if (application.Settings?.InstallationCompleted ?? false)
                {
                    await application.InstallGsi();
                    await application.SaveSettings();
                }

                break;
        }
    }

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}