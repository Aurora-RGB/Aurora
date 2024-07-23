using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AuroraRgb.Settings.Layouts;
using AuroraRgb.Utils;
using Common.Devices;

namespace AuroraRgb.Settings.Controls.Keycaps;

/// <summary>
/// Interaction logic for Control_DefaultKeycapBackglow.xaml
/// </summary>
public partial class Control_DefaultKeycapBackglow
{
    private readonly bool _isImage;

    public Control_DefaultKeycapBackglow()
    {
        InitializeComponent();
    }

    public Control_DefaultKeycapBackglow(KeyboardKey key, string imagePath) : base(key.Tag)
    {
        InitializeComponent();

        Width = key.Width;
        Height = key.Height;

        //Keycap adjustments
        KeyBorder.BorderThickness = new Thickness(string.IsNullOrWhiteSpace(key.Image) ? 1.5 : 0.0);
        KeyBorder.IsEnabled = key.Enabled;

        if (!key.Enabled)
        {
            ToolTipService.SetShowOnDisabled(KeyBorder, true);
            KeyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
        }

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
            GridBackglow.Visibility = Visibility.Hidden;

            if (!File.Exists(imagePath)) return;
            var memStream = new MemoryStream(File.ReadAllBytes(imagePath));
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = memStream;
            image.EndInit();

            if (key.Tag == DeviceKeys.NONE)
                KeyBorder.Background = new ImageBrush(image);
            else
            {
                KeyBorder.Background = Keycap.DefaultColorBrush;
                KeyBorder.OpacityMask = new ImageBrush(image);
            }

            _isImage = true;
        }
    }

    protected override void DrawColor(Color keyColor)
    {
        if (!_isImage)
        {
            KeyCap.Foreground = new SolidColorBrush(keyColor);
            GridBackglow.Background = new SolidColorBrush(keyColor);
        }
        else
        {
            if (AssociatedKey != DeviceKeys.NONE)
                SetGlowColor(keyColor);
        }

        if (KeyBorder.IsEnabled)
        {
            SetGlowColor(!_isImage ? Color.FromArgb(255, 30, 30, 30) : keyColor);
        }
        else
        {
            SetGlowColor(Color.FromArgb(255, 100, 100, 100));
        }
    }

    private Color _currentGlowColor = Color.FromArgb(0, 0, 0, 0);
    private void SetGlowColor(Color color)
    {
        if (!_currentGlowColor.Equals(color))
        {
            KeyBorder.Background = new SolidColorBrush(color);
        }
    }

    private void keyBorder_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is Border)
            OnKeySelected();
        e.Handled = true;
    }

    private void keyBorder_MouseEnter(object? sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
            OnKeySelected();
    }
}