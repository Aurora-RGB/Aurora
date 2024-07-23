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
/// Interaction logic for Control_DefaultKeycap.xaml
/// </summary>
public partial class Control_DefaultKeycap
{
    private readonly bool _isImage;
    private readonly SolidColorBrush _keyCapBackground = new(Colors.Transparent);
    private readonly SolidColorBrush _keyCapForeground = new(Colors.White);
    private readonly SolidColorBrush _keyBorderBorderBrush = new(Colors.Gray);
    private readonly SolidColorBrush _keyBorderBackground = new(Colors.Black);

    public Control_DefaultKeycap()
    {
        InitializeComponent();
    }

    public Control_DefaultKeycap(KeyboardKey key, string imagePath) : base(key.Tag)
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

        KeyBorder.Background = _keyBorderBackground;
        KeyBorder.BorderBrush = _keyBorderBorderBrush;
        KeyCap.Background = _keyCapBackground;
        KeyCap.Foreground = _keyCapForeground;
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
            if (AssociatedKey != DeviceKeys.NONE)
                _keyBorderBackground.Color = keyColor;
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