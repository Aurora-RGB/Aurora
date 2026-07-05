using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Profiles;
using Common.Devices;
using Common.Utils;
using WyvrnChroma;

namespace AuroraRgb.Settings.Layers;

public partial class ChromaEventLayerHandlerProperties : LayerHandlerProperties;

/// <summary>
/// #292 standalone: lights the keyboard from a SetEventName game's events WITHOUT Synapse/Chroma. It reads the
/// live event stream from <see cref="ChromaEventReader"/> (P1), resolves each event to its local Wyvrn
/// <c>.chroma</c> via the WyvrnChroma library, and plays it — each key mapped straight to its exact authored
/// cell in Razer's extended keyboard grid (no resample). A finished one-shot falls back to the game's looping
/// <c>Idle</c> animation.
/// </summary>
[LayerHandlerMeta(Name = "Chroma Event (Wyvrn)", IsDefault = false)]
public sealed class ChromaEventLayerHandler() : LayerHandler<ChromaEventLayerHandlerProperties>("Chroma Event Layer")
{
    // The classic Razer keyboard grid; RazerLayoutMap.GenericKeyboard positions are expressed in it.
    private const int StdRows = 6;
    private const int StdCols = 22;
    private static readonly DeviceKeys[] Keys = Enum.GetValues<DeviceKeys>();

    private readonly object _gate = new();
    private ChromaEventReader? _reader;
    private volatile HapticCatalog? _catalog;
    private ChromaPlayer? _player;     // the active event animation (may be one-shot)
    private ChromaPlayer? _idlePlayer; // the game's looping Idle base
    private DateTime _start = DateTime.UtcNow;
    private string _game = string.Empty;
    private DateTime _lastAliveCheck = DateTime.MinValue;
    private bool _sourceAlive = true;

    protected override UserControl CreateControl() => new();

    public override void Dispose()
    {
        // The reader is an app-lifetime singleton (ChromaEventModule.Reader); layer handlers are recreated on
        // profile/settings changes, so a dangling subscription would leak and keep a dead handler firing.
        if (_reader is not null)
            _reader.EventReceived -= OnEvent;
        _reader = null;
        base.Dispose();
    }

    public override EffectLayer Render(IGameState gameState)
    {
        Wire();

        ChromaPlayer? player;
        DateTime start;
        lock (_gate)
        {
            var elapsed = (DateTime.UtcNow - _start).TotalSeconds;
            player = _player is not null && !_player.IsFinished(elapsed) ? _player : _idlePlayer;
            start = _start;
        }

        if (player is null)
            return EmptyLayer.Instance;

        // The event buffer carries no "game exited" signal, so a looping animation (Idle) would play forever
        // after the game closes. The records do carry the emitter's PID — stop when that process is gone.
        if (!SourceAlive())
        {
            lock (_gate)
            {
                _player = null;
                _idlePlayer = null;
                _game = string.Empty;
            }
            Global.logger.Information("[ChromaEvent/Wyvrn] source process exited; stopping playback [{BuildTag}]",
                ChromaEventModule.BuildTag);
            return EmptyLayer.Instance;
        }

        var frame = player.FrameAt((DateTime.UtcNow - start).TotalSeconds);
        var grid = player.Animation.Grid ?? new ChromaGrid(1, frame.Colors.Count);

        // The keyboard .chroma is authored on Razer's EXTENDED grid (DE_KeyboardExtended, device 3 = 8×24) with
        // the classic 6×22 keyboard block centred inside it. Map each key straight to its authored cell at that
        // offset — NO resample/interpolation — so the wave lands on the exact keys Synapse lights.
        var rowOffset = (grid.Rows - StdRows) / 2;      // 8 -> 1, 6 -> 0
        var colOffset = (grid.Columns - StdCols) / 2;   // 24 -> 1, 22 -> 0

        foreach (var key in Keys)
        {
            if (!RazerLayoutMap.GenericKeyboard.TryGetValue(key, out var pos))
                continue;
            var r = pos[0] + rowOffset;
            var c = pos[1] + colOffset;
            if (r < 0 || r >= grid.Rows || c < 0 || c >= grid.Columns)
                continue;
            var rc = frame.Colors[c + r * grid.Columns];
            var color = CommonColorUtils.FastColor(rc.R, rc.G, rc.B);
            EffectLayer.Set(key, in color);
        }

        return EffectLayer;
    }

    private bool SourceAlive()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastAliveCheck).TotalSeconds < 1)
            return _sourceAlive;
        _lastAliveCheck = now;

        var pid = _reader?.CurrentPid ?? 0;
        if (pid <= 0)
            return _sourceAlive = true; // records without a PID: nothing to check against

        try
        {
            using var _ = System.Diagnostics.Process.GetProcessById(pid);
            _sourceAlive = true;
        }
        catch (ArgumentException)
        {
            _sourceAlive = false;
        }
        return _sourceAlive;
    }

    private void Wire()
    {
        if (_reader is not null)
            return;
        if (!ChromaEventModule.Reader.IsCompletedSuccessfully)
            return;

        _reader = ChromaEventModule.Reader.Result;
        _reader.EventReceived += OnEvent;

        // Resolve the catalog off the render thread (local install first, else the CDN — once).
        _ = Task.Run(() =>
        {
            try
            {
                var result = new HapticCatalogProvider(new HttpClient()).EnsureCatalogAsync().GetAwaiter().GetResult();
                _catalog = new HapticCatalog(result.RootDirectory);
            }
            catch
            {
                // no catalog -> the layer renders nothing
            }
        });
    }

    private void OnEvent(object? sender, ChromaGameEvent e)
    {
        var catalog = _catalog;
        if (catalog is null)
            return;

        lock (_gate)
        {
            if (!string.Equals(e.Game, _game, StringComparison.OrdinalIgnoreCase))
            {
                _game = e.Game;
                _idlePlayer = ResolveIdle(catalog, e.Game);
                Global.logger.Information("[ChromaEvent/Wyvrn] idle base for game='{Game}' {State} [{BuildTag}]",
                    e.Game, _idlePlayer is null ? "MISSING" : $"frames={_idlePlayer.Animation.Frames.Count} loop={_idlePlayer.Loop}",
                    ChromaEventModule.BuildTag);
            }

            var player = Resolve(catalog, e.Game, e.EventName);
            if (player is null)
            {
                Global.logger.Information("[ChromaEvent/Wyvrn] no .chroma for game='{Game}' event='{Event}'", e.Game, e.EventName);
                return;
            }

            Global.logger.Information("[ChromaEvent/Wyvrn] playing game='{Game}' event='{Event}' frames={Frames} loop={Loop} [{BuildTag}]",
                e.Game, e.EventName, player.Animation.Frames.Count, player.Loop, ChromaEventModule.BuildTag);
            _player = player;
            _start = DateTime.UtcNow;
            _sourceAlive = true;
            _lastAliveCheck = DateTime.UtcNow;
        }
    }

    private static ChromaPlayer? Resolve(HapticCatalog catalog, string game, string eventName)
    {
        try
        {
            return catalog.GetEffectForEvent(game, eventName, ChromaDevice.Keyboard)?.CreatePlayer();
        }
        catch
        {
            return null; // unmapped event / missing or corrupt .chroma -> ignore
        }
    }

    private static ChromaPlayer? ResolveIdle(HapticCatalog catalog, string game)
    {
        try
        {
            return catalog.GetIdleEffect(game, ChromaDevice.Keyboard)?.CreatePlayer();
        }
        catch
        {
            return null; // no declared idle / missing or corrupt Idle .chroma -> no base loop
        }
    }
}
