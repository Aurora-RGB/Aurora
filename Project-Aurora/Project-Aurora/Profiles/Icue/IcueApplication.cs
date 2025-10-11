using System;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules;

namespace AuroraRgb.Profiles.Icue;

public sealed class IcueApplication() : Application(new LightEventConfig
{
    Name = "iCUE Apps",
    ID = "icue",
    ProcessNames = [],
    ProfileType = typeof(IcueProfile),
    OverviewControlType = typeof(Control_Icue),
    IconURI = "Resources/icue.png",
    EnableByDefault = true,
    Priority = 5,
})
{
    public override async Task<bool> Initialize(CancellationToken cancellationToken)
    {
        var baseInit = await base.Initialize(cancellationToken);

        IcueModule.AuroraIcueServer.Sdk.GameChanged += IcueSdkGameChanged;
        SetProfileApplication();

        return baseInit;
    }

    private void IcueSdkGameChanged(object? sender, EventArgs e)
    {
        SetProfileApplication();
    }

    private void SetProfileApplication()
    {
        var sdkGameProcess = IcueModule.AuroraIcueServer.Sdk.GameProcess;
        if (string.IsNullOrWhiteSpace(sdkGameProcess))
        {
            Config.ProcessNames = [];
            return;
        }

        Config.ProcessNames = [sdkGameProcess.ToLowerInvariant()];
    }

    public override void Dispose()
    {
        base.Dispose();

        IcueModule.AuroraIcueServer.Sdk.GameChanged -= IcueSdkGameChanged;
    }
}