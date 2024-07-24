using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Utils;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_AnimationFrameItem.xaml
/// </summary>
public partial class Control_AnimationFrameItem
{
    public delegate void DragAdjust(object? sender, double delta);

    public event DragAdjust? LeftSplitterDrag;

    public event DragAdjust? RightSplitterDrag;

    public event DragAdjust? ContentSplitterDrag;

    public event DragAdjust? CompletedDrag;

    public delegate void AnimationFrameItemArgs(object? sender, AnimationFrame? track);

    public event AnimationFrameItemArgs? AnimationFrameItemUpdated;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty ContextFrameProperty = DependencyProperty.Register(nameof(ContextFrame), typeof(AnimationFrame), typeof(Control_AnimationFrameItem));

    public AnimationFrame? ContextFrame
    {
        get => (AnimationFrame)GetValue(ContextFrameProperty);
        set
        {
            SetValue(ContextFrameProperty, value);

            if(value != null)
            {
                Brush bgBrush = new LinearGradientBrush(ColorUtils.DrawingColorToMediaColor(value.Color), Color.FromArgb(0, 0, 0, 0), new Point(0.5, 0), new Point(0.5, 1));
                Brush splitterBrush = new SolidColorBrush(ColorUtils.DrawingColorToMediaColor(value.Color));

                switch (value)
                {
                    case AnimationGradientCircle gradientCircle:
                        bgBrush = gradientCircle.GradientBrush.GetMediaBrush();
                        splitterBrush = gradientCircle.GradientBrush.GetMediaBrush();
                        break;
                    case AnimationFilledGradientRectangle filledGradientRectangle:
                        bgBrush = filledGradientRectangle.GradientBrush.GetMediaBrush();
                        splitterBrush = filledGradientRectangle.GradientBrush.GetMediaBrush();
                        break;
                    case AnimationManualColorFrame:
                        bgBrush = new LinearGradientBrush(Color.FromArgb(255, 100, 100, 100), Color.FromArgb(0, 0, 0, 0), new Point(0.5, 0), new Point(0.5, 1));
                        splitterBrush = Brushes.Black;
                        break;
                }

                DisplayRect.Fill = bgBrush;
                SplitterLeftGrd.Background = splitterBrush;
                SplitterRightGrd.Background = splitterBrush;
            }

            AnimationFrameItemUpdated?.Invoke(this, value);
        }
    }

    public Control_AnimationFrameItem()
    {
        InitializeComponent();
    }

    private void grdSplitterLeft_DragDelta(object? sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        LeftSplitterDrag?.Invoke(this, e.HorizontalChange);
    }

    private void grdSplitterRight_DragDelta(object? sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        RightSplitterDrag?.Invoke(this, e.HorizontalChange);
    }

    private void grdSplitterContent_DragDelta(object? sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        ContentSplitterDrag?.Invoke(this, e.HorizontalChange);
    }

    private void grdSplitter_DragCompleted(object? sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        CompletedDrag?.Invoke(this, 0.0);
    }

    public void SetSelected(bool value)
    {
        //Is selected!
        if (value)
            SelectedRect.Visibility = Visibility.Visible;
        //Deselect
        else
            SelectedRect.Visibility = Visibility.Collapsed;
    }
}