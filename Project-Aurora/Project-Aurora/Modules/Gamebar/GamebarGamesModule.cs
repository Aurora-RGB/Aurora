using System.Threading.Tasks;
using AuroraRgb.Settings;

namespace AuroraRgb.Modules.Gamebar;

public class GamebarGamesModule : AuroraModule
{
    public static GamebarGamesList GamebarGames { get; } = new();
    public static GamebarConfigManager? GamebarConfigManager { get; private set; }

    protected override async Task Initialize()
    {
        var gamebarConfig = await ConfigManager.LoadGamebarConfig();
        GamebarConfigManager = new GamebarConfigManager(gamebarConfig);
        GamebarGames.StartWatching();
    }

    public override async ValueTask DisposeAsync()
    {
        GamebarGames.StopWatching();
        GamebarGames.Dispose();
    }
}