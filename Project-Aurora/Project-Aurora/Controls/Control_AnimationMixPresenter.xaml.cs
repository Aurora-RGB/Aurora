using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Modules;
using Common.Utils;
using TypeExtensions = AuroraRgb.Utils.TypeExtensions;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_AnimationMixPresenter.xaml
/// </summary>
public partial class Control_AnimationMixPresenter
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty ContextMixProperty = DependencyProperty.Register(nameof(ContextMix), typeof(AnimationMix), typeof(Control_AnimationMixPresenter));

    public AnimationMix ContextMix
    {
        get => (AnimationMix)GetValue(ContextMixProperty);
        set
        {
            SetValue(ContextMixProperty, value);

            TracksPanel.Children.Clear();

            foreach (var track in value.GetTracks())
            {
                var newTrack = new Control_AnimationTrackPresenter { ContextTrack = (AnimationTrack)TypeExtensions.TryClone(track.Value) };
                newTrack.AnimationTrackUpdated += NewTrack_AnimationTrackUpdated;
                newTrack.AnimationFrameItemSelected += NewTrack_AnimationFrameItemSelected;

                TracksPanel.Children.Add(newTrack);
                TracksPanel.Children.Add(new Separator());
            }
            AnimationMixUpdated?.Invoke(this, ContextMix);
        }
    }

    public event Control_AnimationFrameItem.AnimationFrameItemArgs? AnimationFrameItemSelected;

    public delegate void AnimationMixRenderedDelegate(object? sender);

    public event AnimationMixRenderedDelegate? AnimationMixRendered;

    public delegate void AnimationMixArgs(object? sender, AnimationMix mix);

    public event AnimationMixArgs? AnimationMixUpdated;

    public Bitmap? RenderedBitmap { get; private set; }

    public float AnimationScale { get; set; } = 1.0f;

    private float _currentPlaybackTime;

    private bool _updateEnabled;

    private readonly SingleConcurrentThread _playbackTimeUpdater;
    private readonly Bitmap _newBitmap = new(Effects.Canvas.Width, Effects.Canvas.Height);

    public Control_AnimationMixPresenter()
    {
        _playbackTimeUpdater = new("AnimationMixPresenter UpdatePlaybackTime", UpdatePlaybackTime);

        InitializeComponent();
    }

    private void NewTrack_AnimationFrameItemSelected(object? sender, AnimationFrame? track)
    {
        AnimationFrameItemSelected?.Invoke(sender, track);
    }

    private void NewTrack_AnimationTrackUpdated(object? sender, AnimationTrack? track)
    {
        if (track == null)
            TracksPanel.Children.Remove(sender as Control_AnimationTrackPresenter);

        ContextMix.Clear();

        foreach (var child in TracksPanel.Children)
        {
            if (child is Control_AnimationTrackPresenter trackPresenter)
            {
                ContextMix.AddTrack(trackPresenter.ContextTrack);
            }
        }

        _playbackTimeUpdater.Trigger();
    }

    private void RenderAnimation()
    {
        Dispatcher.InvokeAsync(() =>
        {
            _currentPlaybackTime += (float)Global.Configuration.UpdateDelay / 1000;
            ScrubberGrid.Margin = new Thickness(ConvertToLocation(_currentPlaybackTime) + 100.0, 0, 0, 0);

            _playbackTimeUpdater.Trigger();
        });
    }

    private async void btnPlayStop_Click(object? sender, RoutedEventArgs e)
    {
        if (_updateEnabled)
            await StopUpdate();
        else
            await StartUpdate();
    }

    private void grdsplitrScrubber_DragDelta(object? sender, DragDeltaEventArgs e)
    {
        var oldMargin = ScrubberGrid.Margin.Left;
        var newMargin = oldMargin + e.HorizontalChange;

        if (newMargin >= 100)
        {
            ScrubberGrid.Margin = new Thickness(newMargin, 0, 0, 0);
            _currentPlaybackTime = ConvertToTime(newMargin - 100);
        }
        else
        {
            ScrubberGrid.Margin = new Thickness(100, 0, 0, 0);
            _currentPlaybackTime = ConvertToTime(0);
        }

        _playbackTimeUpdater.Trigger();
    }

    public void TriggerUpdate()
    {
        _playbackTimeUpdater.Trigger();
    }

    private void UpdatePlaybackTime()
    {
        var seconds = (int)_currentPlaybackTime;
        var milliseconds = (int)((_currentPlaybackTime - seconds) * 1000.0);

        Dispatcher.Invoke(async () =>
        {
            CurrentTimeText.Text = $"{seconds};{milliseconds}";
            var cm = ContextMix;

            await Task.Run(() =>
            {
                using (var g = Graphics.FromImage(_newBitmap))
                {
                    g.Clear(Color.Black);

                    cm.Draw(g, _currentPlaybackTime, new PointF(AnimationScale, AnimationScale));
                }

                RenderedBitmap = _newBitmap;

                AnimationMixRendered?.Invoke(this);
            });
        });
    }

    private double ConvertToLocation(float time)
    {
        return time * 50.0;
    }

    private float ConvertToTime(double loc)
    {
        return (float)(loc / 50.0f);
    }

    private void btnAddTrack_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;
        button.ContextMenu!.IsEnabled = true;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.Placement = PlacementMode.Bottom;
        button.ContextMenu.IsOpen = true;
    }

    private string GetAvailableTrackName(string defaultTrackName)
    {
        var trackCopy = 1;
        var trackName = defaultTrackName;

        if (!ContextMix.ContainsTrack(trackName)) return trackName;
        while (ContextMix.ContainsTrack($"{trackName} ({trackCopy})"))
        {
            trackCopy++;
        }

        trackName = $"{trackName} ({trackCopy})";

        return trackName;
    }

    private void menuitemAddCircleTrack_Click(object? sender, RoutedEventArgs e)
    {
        var newCircleTrack = new AnimationTrack(GetAvailableTrackName("Circle Track"), 0.0f);
        newCircleTrack.SetFrame(0.0f, new AnimationCircle());

        ContextMix = ContextMix.AddTrack(newCircleTrack);
    }

    private void menuitemAddFilledCircleTrack_Click(object? sender, RoutedEventArgs e)
    {
        var newFilledCircleTrack = new AnimationTrack(GetAvailableTrackName("Filled Circle Track"), 0.0f);
        newFilledCircleTrack.SetFrame(0.0f, new AnimationFilledCircle());

        ContextMix = ContextMix.AddTrack(newFilledCircleTrack);
    }

    private void menuitemAddRectangleTrack_Click(object? sender, RoutedEventArgs e)
    {
        var newRectangleTrack = new AnimationTrack(GetAvailableTrackName("Rectangle Track"), 0.0f);
        newRectangleTrack.SetFrame(0.0f, new AnimationRectangle());

        ContextMix = ContextMix.AddTrack(newRectangleTrack);
    }

    private void menuitemAddFilledRectangleTrack_Click(object? sender, RoutedEventArgs e)
    {
        var newFilledRectangleTrack = new AnimationTrack(GetAvailableTrackName("Filled Rectangle Track"), 0.0f);
        newFilledRectangleTrack.SetFrame(0.0f, new AnimationFilledRectangle());

        ContextMix = ContextMix.AddTrack(newFilledRectangleTrack);
    }

    private void menuitemAddLineTrack_Click(object? sender, RoutedEventArgs e)
    {
        var newLineTrack = new AnimationTrack(GetAvailableTrackName("Line Track"), 0.0f);
        newLineTrack.SetFrame(0.0f, new AnimationLine());

        ContextMix = ContextMix.AddTrack(newLineTrack);
    }

    private void menuitemAddManualColorTrack_Click(object? sender, RoutedEventArgs e)
    {
        var newManualColorTrack = new AnimationTrack(GetAvailableTrackName("Manual Color Track"), 0.0f);
        newManualColorTrack.SetFrame(0.0f, new AnimationManualColorFrame());

        ContextMix = ContextMix.AddTrack(newManualColorTrack);
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        await StartUpdate();
    }

    private async Task StartUpdate()
    {
        var lsm = await LightingStateManagerModule.LightningStateManager;
        lsm.PostUpdate += LsmOnPostUpdate;
        _updateEnabled = true;
    }

    private async void UserControl_Unloaded(object? sender, RoutedEventArgs e)
    {
        await StopUpdate();
        Global.effengine.ForceImageRender(null);
    }

    private async Task StopUpdate()
    {
        var lsm = await LightingStateManagerModule.LightningStateManager;
        lsm.PostUpdate -= LsmOnPostUpdate;
        _updateEnabled = false;
    }

    private void LsmOnPostUpdate(object? sender, EventArgs e)
    {
        RenderAnimation();
    }

    private void UserControl_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        var deltaT = 0.001f;

        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            deltaT = 0.01f;

        if (e.Key == Key.Left)
        {
            e.Handled = true;

            if (_currentPlaybackTime - deltaT < 0)
                _currentPlaybackTime = 0.0f;
            else
                _currentPlaybackTime -= deltaT;

            ScrubberGrid.Margin = new Thickness(ConvertToLocation(_currentPlaybackTime) + 100, 0, 0, 0);

            _playbackTimeUpdater.Trigger();
        }
        else if (e.Key == Key.Right)
        {
            e.Handled = true;

            _currentPlaybackTime += deltaT;

            ScrubberGrid.Margin = new Thickness(ConvertToLocation(_currentPlaybackTime) + 100, 0, 0, 0);

            _playbackTimeUpdater.Trigger();
        }
    }
}