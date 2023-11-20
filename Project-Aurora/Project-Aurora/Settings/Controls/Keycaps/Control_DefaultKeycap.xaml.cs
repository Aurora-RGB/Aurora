﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Utils;
using Common.Devices;

namespace Aurora.Settings.Controls.Keycaps;

/// <summary>
/// Interaction logic for Control_DefaultKeycap.xaml
/// </summary>
public partial class Control_DefaultKeycap : IKeycap
{
    private Color _currentColor = Color.FromArgb(0, 0, 0, 0);
    private readonly DeviceKeys _associatedKey = DeviceKeys.NONE;
    private readonly bool _isImage;
    private readonly SolidColorBrush _keyCapBackground = new(Colors.Transparent);
    private readonly SolidColorBrush _keyCapForeground = new(Colors.White);
    private readonly SolidColorBrush _keyBorderBorderBrush = new(Colors.Gray);
    private readonly SolidColorBrush _keyBorderBackground = new(Colors.Black);

    public Control_DefaultKeycap()
    {
        InitializeComponent();
    }

    public Control_DefaultKeycap(KeyboardKey key, string imagePath)
    {
        InitializeComponent();

        _associatedKey = key.Tag;

        Width = key.Width;
        Height = key.Height;

        //Keycap adjustments
        KeyBorder.BorderThickness = new Thickness(string.IsNullOrWhiteSpace(key.Image) ? 1.5 : 0.0);
        KeyBorder.IsEnabled = key.Enabled.Value;

        if (!key.Enabled.Value)
        {
            ToolTipService.SetShowOnDisabled(KeyBorder, true);
            KeyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
        }

        KeyBorder.Background = _keyBorderBackground;
        KeyBorder.BorderBrush = _keyBorderBorderBrush;
        KeyCap.Background = _keyCapBackground;
        KeyCap.Foreground = _keyCapForeground;
        if (string.IsNullOrWhiteSpace(key.Image))
        {
            KeyCap.Text = KeyUtils.GetAutomaticText(_associatedKey) ?? key.VisualName;
            KeyCap.Tag = key.Tag;
            KeyCap.FontSize = key.FontSize;
            KeyCap.Visibility = Visibility.Visible;
        }
        else
        {
            KeyCap.Visibility = Visibility.Hidden;

            KeyCap.Visibility = Visibility.Hidden;

            if (!File.Exists(imagePath)) return;
            var memStream = new MemoryStream(File.ReadAllBytes(imagePath));
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = memStream;
            image.EndInit();

            var imageBrush = new ImageBrush(image);
            if (key.Tag == DeviceKeys.NONE)
            {
                KeyBorder.Background = imageBrush;
            }
            else
            {
                KeyBorder.OpacityMask = imageBrush;
            }

            _isImage = true;
        }
    }

    public DeviceKeys GetKey()
    {
        return _associatedKey;
    }

    public void SetColor(Color keyColor)
    {
        if (Global.key_recorder?.HasRecorded(_associatedKey) ?? false)
        {
            _keyBorderBackground.Color =
                Color.FromArgb(255, 0,
                    (byte)(Math.Min(Math.Pow(Math.Cos(Time.GetMilliSeconds() / 1000.0 * Math.PI) + 0.05, 2.0), 1.0) *
                           255), 0);
            return;
        }

        if (keyColor.Equals(_currentColor))
        {
            return;
        }

        _currentColor = keyColor;

        if (!KeyBorder.IsEnabled) return;

        if (!_isImage)
        {
            if (string.IsNullOrWhiteSpace(KeyCap.Text))
            {
                _keyBorderBorderBrush.Color = keyColor;
                _keyBorderBackground.Color = keyColor;
            }
            else
            {
                _keyCapForeground.Color = keyColor;
                _keyBorderBackground.Color = Color.FromArgb(255, 30, 30, 30);
            }
        }
        else
        {
            if (_associatedKey != DeviceKeys.NONE)
                _keyBorderBackground.Color = keyColor;
        }
    }

    private void keyBorder_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is Border)
            virtualkeyboard_key_selected(_associatedKey);
    }

    private void virtualkeyboard_key_selected(DeviceKeys key)
    {
        if (key == DeviceKeys.NONE || Global.key_recorder == null) return;
        if (Global.key_recorder.HasRecorded(key))
            Global.key_recorder.RemoveKey(key);
        else
            Global.key_recorder.AddKey(key);
    }

    private void keyBorder_MouseEnter(object? sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
            virtualkeyboard_key_selected(_associatedKey);
    }
}