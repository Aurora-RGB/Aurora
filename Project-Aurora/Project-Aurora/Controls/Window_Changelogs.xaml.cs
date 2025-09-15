using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Updates;
using AuroraRgb.Utils;

namespace AuroraRgb.Controls;

public sealed partial class Window_Changelogs : IDisposable
{
    private readonly UpdateModule _updateModule;
    private readonly TransparencyComponent _transparencyComponent;

    public Window_Changelogs(AuroraChangelog[] content, UpdateModule updateModule)
    {
        _updateModule = updateModule;
        InitializeComponent();

        _transparencyComponent = new TransparencyComponent(this, null);

        MarkdownScrollViewer.ItemsSource = content;
    }

    public void Dispose()
    {
        _transparencyComponent.Dispose();
    }

    private void DeleteChangelogs()
    {
        _updateModule.ClearChangelogs();
    }

    private void Window_Changelogs_OnClosed(object? sender, EventArgs e)
    {
        DeleteChangelogs();
    }

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    public void OnLinkClicked(string url)
    {
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not open link: {0}", url);
        }
    }
}