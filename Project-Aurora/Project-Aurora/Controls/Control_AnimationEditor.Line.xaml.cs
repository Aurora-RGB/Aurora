using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine.Animations;

namespace AuroraRgb.Controls;

public partial class Control_AnimationEditor
{
    private void CreateLine(AnimationLine line, StackPanel newPanel, double separatorHeight)
    {
        var varItemColor = new Control_VariableItem
        {
            VariableTitle = "Start Color",
            VariableObject = line.Color
        };
        varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
        var varItemEndColor = new Control_VariableItem
        {
            VariableTitle = "End Color",
            VariableObject = line.EndColor
        };
        varItemEndColor.VariableUpdated += VarItemEndColor_VariableUpdated;
        var varItemWidth = new Control_VariableItem
        {
            VariableTitle = "Width",
            VariableObject = line.Width
        };
        varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
        var varItemStartPositionX = new Control_VariableItem
        {
            VariableTitle = "Start Position X",
            VariableObject = line.StartPoint.X
        };
        varItemStartPositionX.VariableUpdated += VarItemStartPositionX_VariableUpdated;
        varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
        var varItemStartPositionY = new Control_VariableItem
        {
            VariableTitle = "Start Position Y",
            VariableObject = line.StartPoint.Y
        };
        varItemStartPositionY.VariableUpdated += VarItemStartPositionY_VariableUpdated;
        var varItemEndPositionX = new Control_VariableItem
        {
            VariableTitle = "End Position X",
            VariableObject = line.EndPoint.X
        };
        varItemEndPositionX.VariableUpdated += VarItemEndPositionX_VariableUpdated;
        varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
        var varItemEndPositionY = new Control_VariableItem
        {
            VariableTitle = "End Position Y",
            VariableObject = line.EndPoint.Y
        };
        varItemEndPositionY.VariableUpdated += VarItemEndPositionY_VariableUpdated;


        newPanel.Children.Add(varItemColor);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemEndColor);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemWidth);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemStartPositionX);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemStartPositionY);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemEndPositionX);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemEndPositionY);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
    }

    private void VarItemEndColor_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationLine animationLine } frameItem)
            frameItem.ContextFrame = animationLine.SetEndColor((Color)newVariable);
    }

    private void VarItemEndPositionY_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationLine animationLine } frameItem)
            frameItem.ContextFrame = animationLine.SetEndPoint(animationLine.EndPoint with { Y = (float)newVariable });
    }

    private void VarItemEndPositionX_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationLine animationLine } frameItem)
            frameItem.ContextFrame = animationLine.SetEndPoint(animationLine.EndPoint with { X = (float)newVariable });
    }

    private void VarItemStartPositionY_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationLine animationLine } frameItem)
            frameItem.ContextFrame = animationLine.SetStartPoint(animationLine.StartPoint with { Y = (float)newVariable });
    }

    private void VarItemStartPositionX_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationLine animationLine } frameItem)
            frameItem.ContextFrame = animationLine.SetStartPoint(animationLine.StartPoint with { X = (float)newVariable });
    }
}