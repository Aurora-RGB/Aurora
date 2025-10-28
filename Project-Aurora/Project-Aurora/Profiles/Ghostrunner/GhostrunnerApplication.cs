using System;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules;

namespace AuroraRgb.Profiles.Ghostrunner;

public sealed class GhostrunnerApplication() : Application(new LightEventConfig
{
    Name = "Ghostrunner",
    ID = "ghostrunner",
    ProcessNames = [],  // will be set dynamically when iCUE game is "Ghostrunner"
    ProfileType = typeof(GhostrunnerProfile),
    OverviewControlType = typeof(ControlGhostrunner),
    IconURI = "Resources/Ghostrunner.png",
    EnableByDefault = true,
})
{
    public override async Task<bool> Initialize(CancellationToken cancellationToken)
    {
        var baseInit = await base.Initialize(cancellationToken);

        IcueModule.AuroraIcueServer.Gsi.GameChanged += IcueSdkGameChanged;
        SetProfileApplication();

        return baseInit;
    }

    private void IcueSdkGameChanged(object? sender, EventArgs e)
    {
        SetProfileApplication();
    }

    private void SetProfileApplication()
    {
        var sdkGameProcess = IcueModule.AuroraIcueServer.Gsi.GameName;
        if (sdkGameProcess != "Ghostrunner")
        {
            Config.ProcessNames = [];
            return;
        }

        Config.ProcessNames = ["ghostrunner-win64-shipping.exe"];
    }

    public override void Dispose()
    {
        base.Dispose();

        IcueModule.AuroraIcueServer.Gsi.GameChanged -= IcueSdkGameChanged;
    }
}