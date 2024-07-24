using System.Windows.Controls;
using AuroraRgb.EffectsEngine.Animations;

namespace AuroraRgb.Controls;

public partial class Control_AnimationEditor
{
    private void CreateRectangle(AnimationRectangle rectangle, StackPanel newPanel, double separatorHeight)
    {
        var varItemColor = new Control_VariableItem
        {
            VariableTitle = "Color",
            VariableObject = rectangle.Color
        };
        varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
        var varItemWidth = new Control_VariableItem
        {
            VariableTitle = "Width",
            VariableObject = rectangle.Width
        };
        varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
        var varItemPositionX = new Control_VariableItem
        {
            VariableTitle = "Position X",
            VariableObject = rectangle.Dimension.X
        };
        varItemPositionX.VariableUpdated += VarItemPositionX_VariableUpdated;
        var varItemPositionY = new Control_VariableItem
        {
            VariableTitle = "Position Y",
            VariableObject = rectangle.Dimension.Y
        };
        varItemPositionY.VariableUpdated += VarItemPositionY_VariableUpdated;
        var varItemDimensionWidth = new Control_VariableItem
        {
            VariableTitle = "Width",
            VariableObject = rectangle.Dimension.Width
        };
        varItemDimensionWidth.VariableUpdated += VarItemDimensionWidth_VariableUpdated;
        var varItemDimensionHeight = new Control_VariableItem
        {
            VariableTitle = "Height",
            VariableObject = rectangle.Dimension.Height
        };
        varItemDimensionHeight.VariableUpdated += VarItemDimensionHeight_VariableUpdated;


        newPanel.Children.Add(varItemColor);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        if (rectangle is not AnimationFilledRectangle)
        {
            newPanel.Children.Add(varItemWidth);
            newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        }

        newPanel.Children.Add(varItemPositionX);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemPositionY);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemDimensionWidth);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        newPanel.Children.Add(varItemDimensionHeight);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
    }

    private void VarItemDimensionHeight_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is not Control_AnimationFrameItem { ContextFrame: AnimationRectangle animationRectangle } frameItem) return;
        var frameType = animationRectangle.GetType();

        if (frameType == typeof(AnimationRectangle) || frameType == typeof(AnimationFilledRectangle))
        {
            frameItem.ContextFrame = animationRectangle.SetDimension(animationRectangle.Dimension with { Height = (float)newVariable });
        }
        else
        {
            frameItem.ContextFrame = frameItem.ContextFrame.SetDimension(frameItem.ContextFrame.Dimension with { Height = (float)newVariable });
        }
    }

    private void VarItemDimensionWidth_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is not Control_AnimationFrameItem frameItem) return;
        var frameType = frameItem.ContextFrame?.GetType();

        if (frameType == typeof(AnimationRectangle) || frameType == typeof(AnimationFilledRectangle))
        {
            var frame = frameItem.ContextFrame as AnimationRectangle;

            frameItem.ContextFrame = frame?.SetDimension(frame.Dimension with { Width = (float)newVariable });
        }
        else
        {
            frameItem.ContextFrame = frameItem.ContextFrame?.SetDimension(frameItem.ContextFrame.Dimension with { Width = (float)newVariable });
        }
    }

    private void VarItemPositionY_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is not Control_AnimationFrameItem frameItem) return;
        var frameType = frameItem.ContextFrame?.GetType();

        if (frameType == typeof(AnimationRectangle) || frameType == typeof(AnimationFilledRectangle))
        {
            var frame = frameItem.ContextFrame as AnimationRectangle;

            frameItem.ContextFrame = frame?.SetDimension(frame.Dimension with { Y = (float)newVariable });
        }
        else
        {
            frameItem.ContextFrame = frameItem.ContextFrame?.SetDimension(frameItem.ContextFrame.Dimension with { Y = (float)newVariable });
        }
    }

    private void VarItemPositionX_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is not Control_AnimationFrameItem frameItem) return;
        var frameType = frameItem.ContextFrame?.GetType();

        if (frameType == typeof(AnimationRectangle) || frameType == typeof(AnimationFilledRectangle))
        {
            var frame = frameItem.ContextFrame as AnimationRectangle;

            frameItem.ContextFrame = frame?.SetDimension(frame.Dimension with { X = (float)newVariable });
        }
        else
        {
            frameItem.ContextFrame = frameItem.ContextFrame?.SetDimension(frameItem.ContextFrame.Dimension with { X = (float)newVariable });
        }
    }
}