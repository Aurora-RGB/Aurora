using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules.Gamebar;
using AuroraRgb.Profiles.Generic_Application;

namespace AuroraRgb.Profiles.GenericGames;

public sealed class GenericGamesApplication() : Application(new LightEventConfig
{
    Name = "Generic Games",
    ID = "games",
    ProcessNames = [],
    ProfileType = typeof(GenericApplicationProfile),
    OverviewControlType = typeof(Control_GenericGames),
    IconURI = "Resources/controller-icon.png",
    EnableByDefault = false,
    Priority = -10,
})
{
    public override async Task<bool> Initialize(CancellationToken cancellationToken)
    {
        var baseInit = await base.Initialize(cancellationToken);

        Config.ProcessNames = CreateGameListArray();

        GamebarGamesModule.GamebarGames.GameListChanged += GamebarGamesOnGameListChanged;
        if (GamebarGamesModule.GamebarConfigManager != null)
        {
            GamebarGamesModule.GamebarConfigManager.ExcludedProgramsChanged += GamebarGamesOnGameListChanged;
        }

        return baseInit;
    }

    private void GamebarGamesOnGameListChanged(object? sender, EventArgs e)
    {
        Config.ProcessNames = CreateGameListArray();
    }

    private static string[] CreateGameListArray()
    {
        var excludedPrograms = GamebarGamesModule.GamebarConfigManager?.GetExcludedPrograms() ?? [];
        return GamebarGamesModule.GamebarGames.GameExes
            .Except(excludedPrograms)
            .Select(s => s.ToLowerInvariant())
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