using System;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Controls;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic;

[Evaluatable("String State Variable", category: EvaluatableCategory.State)]
public class StringGSIString : GsiEvaluatable<string>
{
    /// <summary>Control assigned to this logic node.</summary>
    public override Visual GetControl() => new GameStateParameterPicker { PropertyType = GSIPropertyType.String }
        .WithBinding(GameStateParameterPicker.ApplicationProperty, new AttachedApplicationBinding())
        .WithBinding(GameStateParameterPicker.SelectedPathProperty, new Binding("VariablePath") { Source = this });

    /// <summary>Attempts to return the string at the given state variable.</summary>
    protected override string Execute(IGameState gameState) => gameState.GetString(VariablePath);
    
    // invalid op execute overrides
    protected override bool ExecuteBool(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override int ExecuteInt(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override double ExecuteDouble(IGameState gameState)
    {
        throw new InvalidOperationException();
    }

    /// <summary>Clones this StringGSIString.</summary>
    public override Evaluatable<string> Clone() => new StringGSIString { VariablePath = VariablePath };
}