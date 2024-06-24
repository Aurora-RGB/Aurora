using System.Threading.Tasks;

namespace AuroraRgb.Profiles;

public abstract class GsiApplication(LightEventConfig config) : Application(config)
{
    public async Task<bool> InstallGsi()
    {
        var result = await DoInstallGsi();
        if (!result)
            return result;
        Settings?.CompleteInstallation();
        return result;
    }
    protected abstract Task<bool> DoInstallGsi();
}