using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Common;
using SourceChord.FluentWPF;
using static AuroraRgb.Utils.DwmApi;

namespace AuroraRgb.Utils;

public sealed class TransparencyComponent : IDisposable
{
    public static bool UseMica { get; } = Environment.OSVersion.Version.Build >= 22000;

    private readonly AcrylicWindow _window;
    private readonly Panel? _bg;
    private readonly RegistryWatcher _lightThemeRegistryWatcher;

    private HwndSource? _hwHandle;
    private readonly Action _setBackground;
    private SimpleColor _color = SimpleColor.FromArgb(1, 1, 1, 1);

    public TransparencyComponent(AcrylicWindow window, Panel? bg)
    {
        _window = window;
        _bg = bg;
        _lightThemeRegistryWatcher = new RegistryWatcher(RegistryHiveOpt.CurrentUser,
            @"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
            "AppsUseLightTheme");
        
        _window.ContentRendered += Window_ContentRendered;
        _window.Loaded += WindowOnLoaded;
        _setBackground = SetBackground;

        _window.WindowStyle = WindowStyle.None;
        _window.AllowsTransparency = true;
        _window.TintColor = Colors.Transparent;
        _window.TintOpacity = 1;
        _window.NoiseOpacity = 0;
        _window.FallbackColor = Color.FromArgb(64, 0, 0, 0);
    }

    private void WindowOnLoaded(object sender, RoutedEventArgs e)
    {
        // Get PresentationSource
        var presentationSource = PresentationSource.FromVisual((Visual)sender);

        // Subscribe to PresentationSource's ContentRendered event
        if (presentationSource != null) presentationSource.ContentRendered += Window_ContentRendered;
        UpdateStyleAttributes();
    }

    private void Window_ContentRendered(object? sender, EventArgs e)
    {
        UpdateStyleAttributes();
    }

    private void UpdateStyleAttributes()
    {
        _lightThemeRegistryWatcher.RegistryChanged -= LightThemeRegistryWatcherOnRegistryChanged;
        _lightThemeRegistryWatcher.RegistryChanged += LightThemeRegistryWatcherOnRegistryChanged;
        _lightThemeRegistryWatcher.StartWatching();
    }

    private void LightThemeRegistryWatcherOnRegistryChanged(object? sender, RegistryChangedEventArgs e)
    {
        if (e.Data is not int)
        {
            return;
        }

        _window.Dispatcher.BeginInvoke(() => { SetTransparencyEffect(0); }, DispatcherPriority.Render);
    }

    private void SetTransparencyEffect(int lightThemeEnabled)
    {
        _hwHandle ??= HwndSource.FromHwnd(new WindowInteropHelper(_window).Handle)!;
        var darkThemeEnabled = lightThemeEnabled == 0;

        if (UseMica && Global.Configuration.AllowTransparency)
        {
            var trueValue = 0x01;
            var falseValue = 0x00;

            AcrylicWindow.SetAcrylicAccentState(_window, AcrylicAccentState.Disabled);

            // Set dark mode before applying the material, otherwise you'll get an ugly flash when displaying the window.
            if (darkThemeEnabled)
                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE,
                    ref trueValue, Marshal.SizeOf(typeof(int)));
            else
                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE,
                    ref falseValue, Marshal.SizeOf(typeof(int)));

            if (Environment.OSVersion.Version.Build >= 22523)
            {
                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
                var mica = 2;
                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref mica, Marshal.SizeOf(typeof(int)));
            }
            else
            {
                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue,
                    Marshal.SizeOf(typeof(int)));
            }

            _window.TintColor = Color.FromArgb(1, 0, 0, 0);
            _window.FallbackColor = Color.FromArgb(64, 0, 0, 0);
        }
        else
        {
            _window.TintColor = Color.FromArgb(240, 128, 128, 128);
            _window.FallbackColor = Color.FromArgb(64, 0, 0, 0);
        }
    }

    public void SetBackgroundColor(SimpleColor a)
    {
        if (_color == a)
        {
            return;
        }
        _color = a;
        _window.Dispatcher.BeginInvoke(_setBackground, DispatcherPriority.DataBind);
    }

    private void SetBackground()
    {
        if (_bg == null)
        {
            return;
        }

        WindowsSystemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

        SolidColorBrush brush;
        if (Global.Configuration.AllowTransparency && UseMica)
        {
            brush = new SolidColorBrush(Color.FromArgb((byte)(_color.A * 64 / 255), _color.R, _color.G, _color.B));
        }
        else
        {
            brush = new SolidColorBrush(Color.FromArgb(255, _color.R, _color.G, _color.B));
        }

        brush.Freeze();
        _bg.Background = brush;
    }

    public void Dispose()
    {
        _window.ContentRendered -= Window_ContentRendered;
        _window.Loaded -= WindowOnLoaded;
        
        _lightThemeRegistryWatcher.Dispose();
        _hwHandle?.Dispose();
    }
}

internal static class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

    private static object? _mDispatcherQueueController;
    public static void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            // one already exists, so we'll just use it.
            return;
        }

        if (_mDispatcherQueueController != null) return;
        DispatcherQueueOptions options;
        options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
        options.threadType = 2;    // DQTYPE_THREAD_CURRENT
        options.apartmentType = 2; // DQTAT_COM_STA

        CreateDispatcherQueueController(options, ref _mDispatcherQueueController);
    }
}