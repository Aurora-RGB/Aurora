using System.Threading.Tasks;
using AuroraRgb.Utils;

namespace AuroraRgb.Modules;

public sealed class PointerUpdateModule : AuroraModule
{
    protected override async Task Initialize()
    {
        if (!Global.Configuration.GetPointerUpdates) return;
        Global.logger.Information("Fetching latest pointers");
        await PointerUpdateUtils.FetchDevPointers("master");
    }

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}