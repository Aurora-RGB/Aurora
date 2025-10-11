using System;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules;

namespace AuroraRgb.Profiles.BlackOps6;

public sealed class Bo6Application() : Application(new LightEventConfig
{
    Name = "Black Ops 6",
    ID = "blackops6",
    ProcessNames = [],  // will be set dynamically when iCUE game is "BlackOps6"
    ProfileType = typeof(Bo6Profile),
    OverviewControlType = typeof(ControlBo6),
    IconURI = "Resources/bo6.png",
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
        if (sdkGameProcess != "BlackOps6")
        {
            Config.ProcessNames = [];
            return;
        }

        Config.ProcessNames = ["cod.exe"];
    }

    public override void Dispose()
    {
        base.Dispose();

        IcueModule.AuroraIcueServer.Gsi.GameChanged -= IcueSdkGameChanged;
    }
}