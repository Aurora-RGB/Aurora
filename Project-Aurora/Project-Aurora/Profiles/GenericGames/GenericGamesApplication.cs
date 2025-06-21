using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules.Gamebar;
using AuroraRgb.Profiles.Desktop;
using AuroraRgb.Profiles.Generic_Application;

namespace AuroraRgb.Profiles.GenericGames;

public sealed class GenericGamesApplication() : Application(new LightEventConfig
{
    Name = "Generic Games",
    ID = "games",
    ProcessNames = [],
    ProfileType = typeof(GenericApplicationProfile),
    OverviewControlType = typeof(Control_GenericGames),
    GameStateType = typeof(DesktopState),
    IconURI = "Resources/controller-icon.png",
    EnableByDefault = false,
    Priority = -10,
})
{
    public override async Task<bool> Initialize(CancellationToken cancellationToken)
    {
        var baseInit = await base.Initialize(cancellationToken);

        var excludedPrograms = GamebarGamesModule.GamebarConfigManager?.GetExcludedPrograms() ?? [];
        Config.ProcessNames = GamebarGamesList.GetGameExes()
            .Except(excludedPrograms)
            .ToArray();

        GamebarGamesModule.GamebarGames.GameListChanged += GamebarGamesOnGameListChanged;
        if (GamebarGamesModule.GamebarConfigManager != null)
        {
            GamebarGamesModule.GamebarConfigManager.ExcludedProgramsChanged += GamebarGamesOnGameListChanged;
        }

        return baseInit;
    }

    private void GamebarGamesOnGameListChanged(object? sender, EventArgs e)
    {
        var excludedPrograms = GamebarGamesModule.GamebarConfigManager?.GetExcludedPrograms() ?? [];
        Config.ProcessNames = GamebarGamesList.GetGameExes()
            .Except(excludedPrograms)
            .ToArray();
    }

    public override void Dispose()
    {
        base.Dispose();
        GamebarGamesModule.GamebarGames.GameListChanged -= GamebarGamesOnGameListChanged;
        if (GamebarGamesModule.GamebarConfigManager != null)
        {
            GamebarGamesModule.GamebarConfigManager.ExcludedProgramsChanged -= GamebarGamesOnGameListChanged;
        }
    }
}