using System;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules;

namespace AuroraRgb.Profiles.BlackOps7;

public sealed class Bo7Application() : Application(new LightEventConfig
{
    Name = "Black Ops 7",
    ID = "blackops7",
    ProcessNames = [],  // will be set dynamically when iCUE game is "BlackOps6"
    ProfileType = typeof(Bo7Profile),
    OverviewControlType = typeof(ControlBo7),
    IconURI = "Resources/bo7.png",
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
        if (sdkGameProcess != "BlackOps7")
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