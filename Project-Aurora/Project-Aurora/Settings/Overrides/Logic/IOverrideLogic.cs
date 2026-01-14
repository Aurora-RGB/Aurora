using System;
using System.ComponentModel;
using System.Drawing;
using AuroraRgb.Profiles;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Interface that states this class can be used for a property for the overrides logic system.
/// Anything implementing this class should have a constructor that takes a `Type` parameter (which is the type of property being edited).
/// </summary>
public interface IOverrideLogic : INotifyPropertyChanged {

    /// <summary>
    /// Evalutes this logic and returns the value of the first lookup which has a truthy condition.
    /// Will return `null` if there are no true conditions.
    /// </summary>
    object? Evaluate(IGameState gameState);
    
    bool EvaluateBool(IGameState gameState, out bool overridden);
    
    double EvaluateDouble(IGameState gameState, out bool overridden);
    
    Rectangle EvaluateRectangle(IGameState gameState, out bool overridden);
    
    Color EvaluateColor(IGameState gameState, out bool overridden);

    /// <summary>
    /// Gets a control for editing this override logic system.
    /// </summary>
    System.Windows.Media.Visual GetControl();
        
    public Type VarType { get; set; }
}