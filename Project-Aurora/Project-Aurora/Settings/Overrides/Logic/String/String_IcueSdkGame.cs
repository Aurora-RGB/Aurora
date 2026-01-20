using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Modules;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic.String;

[Evaluatable("iCUE Sdk Process", category: EvaluatableCategory.Icue)]
public class String_IcueSdkGame : StringEvaluatable
{

    protected override string Execute(IGameState gameState)
    {
        return IcueModule.AuroraIcueServer.Sdk.GameProcess;
    }

    public override Visual GetControl()
    {
        return new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock
            {
                Text = "Icue Sdk Game Process",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(2, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
            });
    }

    public override Evaluatable<string> Clone()
    {
        return new String_IcueSdkGame();
    }
}