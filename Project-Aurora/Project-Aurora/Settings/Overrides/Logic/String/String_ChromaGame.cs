using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic.String;

[Evaluatable("Chroma Game Process", category: EvaluatableCategory.Icue)]
public class String_ChromaSdkGame : StringEvaluatable
{

    protected override string Execute(IGameState gameState)
    {
        return RzHelper.CurrentAppExecutable ?? string.Empty;
    }

    public override Visual GetControl()
    {
        return new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock
            {
                Text = "Chroma Game Process",
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