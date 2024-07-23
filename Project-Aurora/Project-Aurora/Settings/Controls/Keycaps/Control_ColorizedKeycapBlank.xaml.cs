using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AuroraRgb.Settings.Layouts;
using Common.Devices;

namespace AuroraRgb.Settings.Controls.Keycaps;

/// <summary>
/// Interaction logic for Control_ColorizedKeycapBlank.xaml
/// </summary>
public partial class Control_ColorizedKeycapBlank
{
    private readonly bool _isImage;
    private readonly SolidColorBrush _keyBorderBorderBrush = new(Colors.Gray);
    private readonly SolidColorBrush _keyBorderBackground = new(Colors.Black);

    public Control_ColorizedKeycapBlank()
    {
        InitializeComponent();
    }

    public Control_ColorizedKeycapBlank(KeyboardKey key, string imagePath) : base(key.Tag)
    {
        InitializeComponent();

        Width = key.Width;
        Height = key.Height;

        //Keycap adjustments
        KeyBorder.BorderThickness = new Thickness(string.IsNullOrWhiteSpace(key.Image) ? 1.5 : 0.0);

        var keyEnabled = key.Enabled;
        KeyBorder.IsEnabled = keyEnabled;

        if (!keyEnabled)
        {
            ToolTipService.SetShowOnDisabled(KeyBorder, true);
            KeyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
        }

        KeyBorder.BorderBrush = _keyBorderBorderBrush;
        KeyBorder.Background = _keyBorderBackground;

        if (string.IsNullOrWhiteSpace(key.Image)) return;
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
            KeyBorder.Background = _keyBorderBackground;
            KeyBorder.OpacityMask = new ImageBrush(image);
        }

        _isImage = true;
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
        e.Handled = true;
    }

    private void keyBorder_MouseEnter(object? sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
            OnKeySelected();
    }
}