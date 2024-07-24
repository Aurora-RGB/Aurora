using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AuroraRgb.EffectsEngine.Animations;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_AnimationTrackPresenter.xaml
/// </summary>
public partial class Control_AnimationTrackPresenter
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty ContextTrackProperty = DependencyProperty.Register(nameof(ContextTrack), typeof(AnimationTrack), typeof(Control_AnimationTrackPresenter));

    public AnimationTrack? ContextTrack
    {
        get => (AnimationTrack)GetValue(ContextTrackProperty);
        set
        {
            SetValue(ContextTrackProperty, value);

            if(value != null)
                UpdateControls();

            AnimationTrackUpdated?.Invoke(this, ContextTrack);
        }
    }

    public delegate void AnimationTrackArgs(object? sender, AnimationTrack? track);

    public event AnimationTrackArgs? AnimationTrackUpdated;

    public event Control_AnimationFrameItem.AnimationFrameItemArgs? AnimationFrameItemSelected;

    private double _height = 100.0;

    private double ConvertToLocation(float time, float shift = 0.0f)
    {
        return (time + shift) * 50.0;
    }

    private float ConvertToTime(double loc, float shift = 0.0f)
    {
        return (float)(loc / 50.0f) - shift;
    }

    public Control_AnimationTrackPresenter()
    {
        InitializeComponent();
    }

    private void UpdateControls()
    {
        TrackNameText.Text = ContextTrack.GetName();

        if (ContextTrack.SupportedAnimationType == typeof(AnimationFilledCircle))
        {
            this.TrackTypeImage.Source = new BitmapImage(new Uri(@"/AuroraRgb;component/Resources/FreeForm_CircleFilled.png", UriKind.Relative));
            this.TrackTypeImage.ToolTip = "Filled Circle Track";
        }
        else if (ContextTrack.SupportedAnimationType == typeof(AnimationCircle))
        {
            this.TrackTypeImage.Source = new BitmapImage(new Uri(@"/AuroraRgb;component/Resources/FreeForm_Circle.png", UriKind.Relative));
            this.TrackTypeImage.ToolTip = "Circle Track";
        }
        else if (ContextTrack.SupportedAnimationType == typeof(AnimationFilledRectangle))
        {
            this.TrackTypeImage.Source = new BitmapImage(new Uri(@"/AuroraRgb;component/Resources/FreeForm_RectangleFilled.png", UriKind.Relative));
            this.TrackTypeImage.ToolTip = "Filled Rectangle Track";
        }
        else if (ContextTrack.SupportedAnimationType == typeof(AnimationRectangle))
        {
            this.TrackTypeImage.Source = new BitmapImage(new Uri(@"/AuroraRgb;component/Resources/FreeForm_Rectangle.png", UriKind.Relative));
            this.TrackTypeImage.ToolTip = "Rectangle Track";
        }
        else if (ContextTrack.SupportedAnimationType == typeof(AnimationLine))
        {
            this.TrackTypeImage.Source = new BitmapImage(new Uri(@"/AuroraRgb;component/Resources/FreeForm_Line.png", UriKind.Relative));
            this.TrackTypeImage.ToolTip = "Line Track";
        }
        else if (ContextTrack.SupportedAnimationType == typeof(AnimationManualColorFrame))
        {
            this.TrackTypeImage.Source = new BitmapImage(new Uri(@"/AuroraRgb;component/Resources/FreeForm_ManualColor.png", UriKind.Relative));
            this.TrackTypeImage.ToolTip = "Manual Color Track";
        }

        TrackItemsGrid.Children.Clear();

        foreach (var kvp in ContextTrack.GetAnimations())
        {
            var newFrame = new Control_AnimationFrameItem() { ContextFrame = kvp.Value, Margin = new Thickness(ConvertToLocation(kvp.Key, ContextTrack.GetShift()), 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, Width = ConvertToLocation(kvp.Value.Duration) };
            newFrame.LeftSplitterDrag += Control_AnimationFrameItem_LeftSplitterDrag;
            newFrame.RightSplitterDrag += Control_AnimationFrameItem_RightSplitterDrag;
            newFrame.ContentSplitterDrag += Control_AnimationFrameItem_ContentSplitterDrag;
            newFrame.CompletedDrag += Control_AnimationFrameItem_CompletedDrag;
            newFrame.PreviewMouseDown += Control_AnimationFrameItem_PreviewMouseDown;
            newFrame.AnimationFrameItemUpdated += Control_AnimationFrameItem_AnimationFrameItemUpdated;

            TrackItemsGrid.Children.Add(newFrame);
        }
    }

    private void Control_AnimationFrameItem_AnimationFrameItemUpdated(object? sender, AnimationFrame? frame)
    {
        if(frame == null)
        {
            TrackItemsGrid.Children.Remove(sender as Control_AnimationFrameItem);

            UpdateAnimationTrack();
        }
        else
        {
            AnimationTrackUpdated?.Invoke(this, ContextTrack);
        }
            
        //UpdateAnimationTrack();
        //UpdateControls();
    }

    private void Control_AnimationFrameItem_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        //Selected
        AnimationFrameItemSelected?.Invoke(sender, (sender as Control_AnimationFrameItem).ContextFrame);
    }

    private void Control_AnimationFrameItem_CompletedDrag(object? sender, double delta)
    {
        UpdateAnimationTrack();
    }

    private void Control_AnimationFrameItem_LeftSplitterDrag(object? sender, double delta)
    {
        var oldMargin = (sender as Control_AnimationFrameItem).Margin.Left;
        var oldWidth = (sender as Control_AnimationFrameItem).Width;
        var newMargin = oldMargin + delta;
        var newWidth = oldWidth - delta;

        if (newWidth >= 0 && newMargin >= 0 && !CheckControlOverlap(sender as Control_AnimationFrameItem, delta, -delta))
        {
            (sender as Control_AnimationFrameItem).Width = newWidth;
            (sender as Control_AnimationFrameItem).Margin = new Thickness(newMargin, 0, 0, 0);

            (sender as Control_AnimationFrameItem).ContextFrame.SetDuration(ConvertToTime(newWidth));
            ContextTrack.RemoveFrame(ConvertToTime(oldMargin, ContextTrack.GetShift()));
            ContextTrack.SetFrame(ConvertToTime(newMargin, ContextTrack.GetShift()), (sender as Control_AnimationFrameItem).ContextFrame);
        }
    }

    private void Control_AnimationFrameItem_RightSplitterDrag(object? sender, double delta)
    {
        var oldWidth = (sender as Control_AnimationFrameItem).Width;
        var newWidth = oldWidth + delta;

        if(newWidth >= 0 && !CheckControlOverlap(sender as Control_AnimationFrameItem, 0, delta))
        {
            (sender as Control_AnimationFrameItem).Width = newWidth;

            var time = ConvertToTime(((sender as Control_AnimationFrameItem).Margin.Left), ContextTrack.GetShift());

            (sender as Control_AnimationFrameItem).ContextFrame.SetDuration(ConvertToTime(newWidth));
            ContextTrack.RemoveFrame(ConvertToTime(time, ContextTrack.GetShift()));
            ContextTrack.SetFrame(ConvertToTime(time, ContextTrack.GetShift()), (sender as Control_AnimationFrameItem).ContextFrame);
        }
    }

    private void Control_AnimationFrameItem_ContentSplitterDrag(object? sender, double delta)
    {
        var oldMargin = (sender as Control_AnimationFrameItem).Margin.Left;
        var newMargin = oldMargin + delta;

        if (newMargin >= 0 && !CheckControlOverlap(sender as Control_AnimationFrameItem, delta))
        {
            (sender as Control_AnimationFrameItem).Margin = new Thickness(newMargin, 0, 0, 0);

            (sender as Control_AnimationFrameItem).ContextFrame.SetDuration(ConvertToTime((sender as Control_AnimationFrameItem).Width));
            ContextTrack.RemoveFrame(ConvertToTime(oldMargin, ContextTrack.GetShift()));
            ContextTrack.SetFrame(ConvertToTime(newMargin, ContextTrack.GetShift()), (sender as Control_AnimationFrameItem).ContextFrame);
        }
    }

    private bool CheckControlOverlap(Control_AnimationFrameItem item, double leftMarginDelta = 0.0, double widthDelta = 0.0)
    {
        var itemPosition = new Rect();
        itemPosition.Location = item.PointToScreen(new Point(leftMarginDelta + 0, 0));
        itemPosition.Height = item.ActualHeight;
        itemPosition.Width = item.ActualWidth + widthDelta;

        var doesIntersect = false;

        foreach (var child in TrackItemsGrid.Children)
        {
            if (child is Control_AnimationFrameItem && (child as Control_AnimationFrameItem) != item)
            {
                var childPos = new Rect();
                childPos.Location = (child as Control_AnimationFrameItem).PointToScreen(new Point(0, 0));
                childPos.Height = (child as Control_AnimationFrameItem).ActualHeight;
                childPos.Width = (child as Control_AnimationFrameItem).ActualWidth;

                if (itemPosition.IntersectsWith(childPos))
                {
                    doesIntersect = true;
                    break;
                }
            }
        }

        return doesIntersect;
    }

    private void UpdateAnimationTrack()
    {
        ContextTrack.Clear();

        foreach (var child in TrackItemsGrid.Children)
        {
            if (child is Control_AnimationFrameItem)
            {
                var item = (child as Control_AnimationFrameItem);

                if (item.ContextFrame != null)
                    ContextTrack.SetFrame(ConvertToTime(item.Margin.Left, ContextTrack.GetShift()), item.ContextFrame);
            }
        }

        AnimationTrackUpdated?.Invoke(this, ContextTrack);
    }

    private void Expander_Collapsed(object? sender, RoutedEventArgs e)
    {
        _height = this.Height;
        this.Height = 50;
    }

    private void Expander_Expanded(object? sender, RoutedEventArgs e)
    {
        this.Height = _height;
    }

    private void gridTrackItems_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            var mouseLoc = e.GetPosition(sender as Grid);

            var time = ConvertToTime(mouseLoc.X, ContextTrack.GetShift());

            ContextTrack.SetFrame(time, ContextTrack.GetFrame(time).GetCopy());

            UpdateControls();
        }
    }

    private void grdsplitHeightAdjust_DragDelta(object? sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        var oldHeight = this.ActualHeight;
        var newHeight = oldHeight + e.VerticalChange;

        if (newHeight >= 75.0)
            this.Height = newHeight;
    }

    private void btnRemoveTrack_Click(object? sender, RoutedEventArgs e)
    {
        ContextTrack = null;
    }

    private void txtboxTrackNameEdit_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if(ContextTrack != null)
        {
            ContextTrack.SetName(TrackNameEditText.Text);
            TrackNameText.Text = TrackNameEditText.Text;
        }
    }

    private void txtblkTrackName_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            TrackNameEditText.Text = TrackNameText.Text;
            TrackNameText.Visibility = Visibility.Collapsed;
            TrackEditCanvas.Visibility = Visibility.Visible;
        }
    }

    private void txtboxTrackNameEdit_LostFocus(object? sender, RoutedEventArgs e)
    {
        TrackNameText.Visibility = Visibility.Visible;
        TrackEditCanvas.Visibility = Visibility.Collapsed;
    }

    private void txtboxTrackNameEdit_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TrackNameText.Visibility = Visibility.Visible;
            TrackEditCanvas.Visibility = Visibility.Collapsed;
        }
    }
}