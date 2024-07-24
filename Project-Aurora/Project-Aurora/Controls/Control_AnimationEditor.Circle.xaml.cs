using System.Windows.Controls;
using AuroraRgb.EffectsEngine.Animations;

namespace AuroraRgb.Controls;

public partial class Control_AnimationEditor
{
    private void CreateCircle(AnimationCircle circle, StackPanel newPanel, double separatorHeight)
    {
        var varItemColor = new Control_VariableItem
        {
            VariableTitle = "Color",
            VariableObject = circle.Color
        };
        varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
        var varItemWidth = new Control_VariableItem
        {
            VariableTitle = "Width",
            VariableObject = circle.Width
        };
        varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
        var varItemCenterX = new Control_VariableItem
        {
            VariableTitle = "Center X",
            VariableObject = circle.Center.X
        };
        varItemCenterX.VariableUpdated += VarItemCenterX_VariableUpdated;
        var varItemCenterY = new Control_VariableItem
        {
            VariableTitle = "Center Y",
            VariableObject = circle.Center.Y
        };
        varItemCenterY.VariableUpdated += VarItemCenterY_VariableUpdated;
        var varItemDimensionRadius = new Control_VariableItem
        {
            VariableTitle = "Radius",
            VariableObject = circle.Radius
        };
        varItemDimensionRadius.VariableUpdated += VarItemDimensionRadius_VariableUpdated;

        newPanel.Children.Add(varItemColor);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        if (circle is not AnimationFilledCircle)
        {
            newPanel.Children.Add(varItemWidth);
            newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        }

        newPanel.Children.Add(varItemCenterX);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemCenterY);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemDimensionRadius);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
    }

    private void VarItemDimensionRadius_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationCircle animationCircle } frameItem)
            frameItem.ContextFrame = animationCircle.SetRadius((float)newVariable);
    }

    private void VarItemCenterY_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationCircle animationCircle } frameItem)
            frameItem.ContextFrame = animationCircle.SetCenter(animationCircle.Center with { Y = (float)newVariable });
    }

    private void VarItemCenterX_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem { ContextFrame: AnimationCircle animationCircle } item)
            item.ContextFrame = animationCircle.SetCenter(animationCircle.Center with { X = (float)newVariable });
    }
}