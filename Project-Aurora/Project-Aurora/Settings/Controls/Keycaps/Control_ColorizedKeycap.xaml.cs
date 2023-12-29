﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Utils;
using Common.Devices;

namespace Aurora.Settings.Controls.Keycaps;

/// <summary>
/// Interaction logic for Control_ColorizedKeycap.xaml
/// </summary>
public partial class Control_ColorizedKeycap
{
    private static readonly SolidColorBrush DisabledKeyColor = new(Color.FromArgb(255, 100, 100, 100));
    private readonly bool _isImage;
    private readonly SolidColorBrush _keyBorderBorderBrush = new(Colors.Gray);
    private readonly SolidColorBrush _keyBorderBackground = new(Colors.Black);

    static Control_ColorizedKeycap()
    {
        DisabledKeyColor.Freeze();
    }

    public Control_ColorizedKeycap()
    {
        InitializeComponent();
    }

    public Control_ColorizedKeycap(KeyboardKey key, string imagePath) : base(key.Tag)
    {
        InitializeComponent();

        Width = key.Width;
        Height = key.Height;

        //Keycap adjustments
        KeyBorder.BorderThickness = new Thickness(string.IsNullOrWhiteSpace(key.Image) ? 1.5 : 0.0);

        var keyEnabled = key.Enabled.GetValueOrDefault(true);
        KeyBorder.IsEnabled = keyEnabled;

        if (!keyEnabled)
        {
            ToolTipService.SetShowOnDisabled(KeyBorder, true);
            KeyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
            KeyBorder.Background = DisabledKeyColor;
        }

        KeyBorder.BorderBrush = _keyBorderBorderBrush;
        KeyBorder.Background = _keyBorderBackground;
        if (string.IsNullOrWhiteSpace(key.Image))
        {
            KeyCap.Text = KeyUtils.GetAutomaticText(AssociatedKey) ?? key.VisualName;
            KeyCap.Tag = key.Tag;
            KeyCap.FontSize = key.FontSize;
            KeyCap.Visibility = Visibility.Visible;
        }
        else
        {
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

    protected override void DrawColor(Color keyColor)
    {
        if (!KeyBorder.IsEnabled) return;

        if (_isImage)
        {
            _keyBorderBackground.Color = keyColor;
        }
        else
        {
            _keyBorderBackground.Color = keyColor * 0.6f;
            _keyBorderBorderBrush.Color = keyColor;
        }
    }

    private void keyBorder_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is Border)
            OnKeySelected();
    }

    private void keyBorder_MouseEnter(object? sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
            OnKeySelected();
    }
}