using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Gamebar;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic.Boolean;

/// <summary>
/// Evaluatable that returns true/false depending on whether the given process name is running.
/// </summary>
[Evaluatable("A Game Is Running", category: EvaluatableCategory.Misc)]
public class BooleanGameRunning : BoolEvaluatable
{
    public override Visual GetControl()
    {
        return new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new Label { Content = "A Game Runs" });
    }

    protected override bool Execute(IGameState gameState)
    {
        return GamebarGamesModule.GamebarGames
            .GameExes
            .Any(processName => ProcessesModule.RunningProcessMonitor.Result.IsProcessRunning(processName));
    }

    public override Evaluatable<bool> Clone() => new BooleanProcessRunning();
}