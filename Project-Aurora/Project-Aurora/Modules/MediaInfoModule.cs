using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.Media;

namespace AuroraRgb.Modules;

public sealed class MediaInfoModule : AuroraModule
{
    private MediaMonitor? _mediaMonitor;

    protected override async Task Initialize()
    {
        Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        
        if (!Global.Configuration.EnableMediaInfo)
        {
            return;
        }
        try
        {
            _mediaMonitor = new MediaMonitor();
        }
        catch (Exception e)
        {
            MessageBox.Show("Media Info module could not be loaded.\nMedia playback data will not be detected.",
                "Aurora - Warning");
            Global.logger.Error(e, "MediaInfo error");
        }
    }

    private void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Global.Configuration.EnableMediaInfo))
        {
            return;
        }

        if (Global.Configuration.EnableMediaInfo)
        {
            _mediaMonitor ??= new MediaMonitor();
        }
        else
        {
            _mediaMonitor?.Dispose();
            _mediaMonitor = null;
        }
    }

    public override ValueTask DisposeAsync()
    {
        _mediaMonitor?.Dispose();
        _mediaMonitor = null;

        return ValueTask.CompletedTask;
    }
}