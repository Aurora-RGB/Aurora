using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Settings.Controls.Keycaps;
using Common.Devices;
using static AuroraRgb.Controls.Control_AnimationMixPresenter;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_AnimationEditor.xaml
/// </summary>
public partial class Control_AnimationEditor
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty AnimationMixProperty = DependencyProperty.Register(nameof(AnimationMix), typeof(AnimationMix), typeof(Control_AnimationEditor));

    public AnimationMix AnimationMix
    {
        get => (AnimationMix)GetValue(AnimationMixProperty);
        set
        {
            SetValue(AnimationMixProperty, value);

            AnimMixer.ContextMix = value;
            AnimationMixUpdated?.Invoke(this, value);
        }
    }

    public event AnimationMixArgs? AnimationMixUpdated;

    private UIElement? _selectedFrameItem;

    private Color _primaryManualColor = Color.Blue;
    private Color _secondaryManualColor = Color.Transparent;

    public Control_AnimationEditor()
    {
        InitializeComponent();

        UpdateVirtualKeyboard();
        UpdateScale(KeyboardOverlayPreview.Width);

        Global.kbLayout.KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;
    }

    private void KbLayout_KeyboardLayoutUpdated(object? sender)
    {
        UpdateVirtualKeyboard();
    }

    private async void UpdateVirtualKeyboard()
    {
        var virtualKb = await Global.kbLayout.AbstractVirtualKeyboard;

        KeyboardGrid.Children.Clear();
        KeyboardGrid.Children.Add(virtualKb);
        KeyboardGrid.Children.Add(new LayerEditor());

        KeyboardGrid.Width = virtualKb.Width;
        KeyboardGrid.Height = virtualKb.Height;

        KeyboardGrid.UpdateLayout();

        AnimationView.MaxWidth = virtualKb.Width + 50;
        AnimationView.MaxHeight = virtualKb.Height + 50;
        AnimationView.UpdateLayout();

        UpdateLayout();

        //Generate a new mapping
        foreach (FrameworkElement child in virtualKb.Children)
        {
            if (child is not Keycap keycap || keycap.GetKey() == DeviceKeys.NONE) continue;
            keycap.PreviewMouseLeftButtonDown += KeyboardKey_PreviewMouseLeftButtonDown;
            keycap.PreviewMouseRightButtonDown += KeyboardKey_PreviewMouseRightButtonDown;
        }
    }

    private void KeyboardKey_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if ((_selectedFrameItem as Control_AnimationFrameItem)?.ContextFrame is not AnimationManualColorFrame || sender is not Keycap keycap) return;
        SetKeyColor(keycap.GetKey(), _primaryManualColor);

        AnimMixer.TriggerUpdate();
    }

    private void KeyboardKey_PreviewMouseRightButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if ((_selectedFrameItem as Control_AnimationFrameItem)?.ContextFrame is not AnimationManualColorFrame || sender is not Keycap keycap) return;
        SetKeyColor(keycap.GetKey(), _secondaryManualColor);

        AnimMixer.TriggerUpdate();
    }

    private void SetKeyColor(DeviceKeys key, Color color)
    {
        if (_selectedFrameItem is not Control_AnimationFrameItem { ContextFrame: AnimationManualColorFrame colorFrame }) return;
        colorFrame.SetKeyColor(key, color);
    }

    private void animMixer_AnimationMixRendered(object? sender)
    {
        if (sender is not Control_AnimationMixPresenter animationMixPresenter)
        {
            return;
        }
        Dispatcher.Invoke(() =>
        {
            var renderedBitmap = animationMixPresenter.RenderedBitmap!;
                
            using var memory = new MemoryStream();
            renderedBitmap.Save(memory, ImageFormat.Bmp);

            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            KeyboardOverlayPreview.Source = bitmapImage;
            UpdateScale(KeyboardOverlayPreview.Width);
        });
    }

    private void keyboard_overlayPreview_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateScale(e.NewSize.Width);
    }

    private float _prevScale;
    private void UpdateScale(double width)
    {
        if (!double.IsPositive(width))
        {
            return;
        }
        var scale = (float)(width / Effects.Canvas.Width);
        if (scale < 1.0f)
            scale = 1.0f;
        if(_prevScale <= 0.01)
        {
            AnimMixer.AnimationScale = scale;

            _prevScale = scale;
        }
        else if (Math.Abs(_prevScale - scale) > 0.001)
        {

            AnimMixer.AnimationScale = scale;

            HorizontalPixelsRuler.MarkSize = scale;
            VerticalPixelsRuler.MarkSize = scale;

            _prevScale = scale;
        }
    }

    private void animMixer_AnimationFrameItemSelected(object? sender, AnimationFrame frame)
    {
        //Deselect old frame
        if (_selectedFrameItem is Control_AnimationFrameItem frameItem)
            frameItem.SetSelected(false);

        if (sender is not Control_AnimationFrameItem animationFrameItem)
        {
            return;
        }
        _selectedFrameItem = animationFrameItem;
        animationFrameItem.SetSelected(true);

        var newPanel = new StackPanel();

        var skipExtra = false;

        double separatorHeight = 3;

        switch (frame)
        {
            //Add default options
            case AnimationCircle circle:
            {
                CreateCircle(circle, newPanel, separatorHeight);
                break;
            }
            case AnimationRectangle rectangle:
            {
                CreateRectangle(rectangle, newPanel, separatorHeight);
                break;
            }
            case AnimationLine line:
            {
                CreateLine(line, newPanel, separatorHeight);
                break;
            }
            case AnimationManualColorFrame:
            {
                skipExtra = true;

                var varItemPrimaryManualColor = new Control_VariableItem
                {
                    VariableTitle = "Primary Color",
                    VariableObject = _primaryManualColor
                };
                varItemPrimaryManualColor.VariableUpdated += VarItemPrimaryManualColor_VariableUpdated;
                var varItemSecondaryManualColor = new Control_VariableItem
                {
                    VariableTitle = "Secondary Color",
                    VariableObject = _secondaryManualColor
                };
                varItemSecondaryManualColor.VariableUpdated += VarItemSecondaryManualColor_VariableUpdated;

                var btnClearColors = new Button
                {
                    Content = "Clear Frame Colors",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                btnClearColors.Click += BtnClearColors_Click;

                //Color picker

                newPanel.Children.Add(varItemPrimaryManualColor);
                newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemSecondaryManualColor);
                newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(btnClearColors);
                newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
                break;
            }
        }

        if (!skipExtra)
        {
            var varItemAngle = new Control_VariableItem
            {
                VariableTitle = "Angle",
                VariableObject = frame.Angle
            };
            varItemAngle.VariableUpdated += VarItemAngle_VariableUpdated;

            newPanel.Children.Add(varItemAngle);
            newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });
        }

        var varItemTransitionType = new Control_VariableItem
        {
            VariableTitle = "Transition Type",
            VariableObject = frame.TransitionType
        };
        varItemTransitionType.VariableUpdated += VarItemTransitionType_VariableUpdated;

        newPanel.Children.Add(varItemTransitionType);
        newPanel.Children.Add(new Separator { Height = separatorHeight, Opacity = 0 });

        var btnRemoveFrame = new Button
        {
            Content = "Remove Frame",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        btnRemoveFrame.Click += BtnRemoveFrame_Click;

        newPanel.Children.Add(btnRemoveFrame);

        PropertiesGrpbx.Content = newPanel;
    }

    private void BtnClearColors_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedFrameItem is not Control_AnimationFrameItem frameItem) return;
        var frameType = frameItem.ContextFrame?.GetType();

        if (frameType != typeof(AnimationManualColorFrame)) return;
        var frame = frameItem.ContextFrame as AnimationManualColorFrame;

        frame?.SetBitmapColors([]);

        AnimMixer.TriggerUpdate();
    }

    private void VarItemSecondaryManualColor_VariableUpdated(object? sender, object? newVariable)
    {
        _secondaryManualColor = (Color)newVariable;
    }

    private void VarItemPrimaryManualColor_VariableUpdated(object? sender, object? newVariable)
    {
        _primaryManualColor = (Color)newVariable;
    }

    private void VarItemAngle_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem frameItem)
            frameItem.ContextFrame = frameItem.ContextFrame?.SetAngle((float)newVariable);
    }

    private void VarItemTransitionType_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem frameItem)
            frameItem.ContextFrame = frameItem.ContextFrame?.SetTransitionType((AnimationFrameTransitionType)newVariable);
    }

    private void BtnRemoveFrame_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem frameItem)
            frameItem.ContextFrame = null;

        PropertiesGrpbx.Content = null;
    }

    private void VarItemWidth_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem frameItem)
            frameItem.ContextFrame = frameItem.ContextFrame?.SetWidth((int)newVariable);
    }

    private void VarItemColor_VariableUpdated(object? sender, object? newVariable)
    {
        if (_selectedFrameItem is Control_AnimationFrameItem frameItem)
            frameItem.ContextFrame = frameItem.ContextFrame?.SetColor((Color)newVariable);
    }

    private void animMixer_AnimationMixUpdated(object? sender, AnimationMix mix)
    {
        SetValue(AnimationMixProperty, mix);

        AnimationMixUpdated?.Invoke(this, mix);
    }

    private void UserControl_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete || _selectedFrameItem is not Control_AnimationFrameItem frameItem) return;
        frameItem.ContextFrame = null;

        PropertiesGrpbx.Content = null;
    }
}