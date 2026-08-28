using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Common.Devices;
using HidSharp;
using Windows.Media.Control;

namespace AuroraDeviceManager.Devices.SteelSeries;

// gamesense-essentials parity for the Gen 3 OLED (128x40, 1bpp), driven from the device
// update loop. Content priority: volume slider (briefly, after a volume change) >
// song info while media plays (with a periodic clock window) > clock with day/date.
// Frame format: unnumbered feature report [0x00, 0x61, <SSD1306 page-major bitmap>].
internal sealed class ApexGen3Oled
{
    private const int Width = 128;
    private const int Height = 40;
    private const byte Command = 0x61;
    private const int VolumeOverlayMs = 1500;
    private const int MediaPollMs = 2000;
    private const int PeriodicClockPeriodS = 30; // last 5s of every 30s window shows the clock

    private bool _clock, _clockIcon, _clockPeriodic, _volume, _song, _songIcon, _songFlip;
    private int _tickMs = 250;
    private int _scrollHoldMs = 2000;

    private byte[] _lastFrame = [];
    private bool _sendErrorLogged;
    private bool _drawErrorLogged;

    private float _lastVol = -1;
    private bool _lastMute;
    private DateTime _volShownUntil = DateTime.MinValue;
    private CoreAudioVolume? _coreAudio;
    private bool _volBroken;

    private DateTime _lastMediaPoll = DateTime.MinValue;
    private GlobalSystemMediaTransportControlsSessionManager? _smtc;
    private volatile string _title = "";
    private volatile string _artist = "";
    private volatile bool _playing;
    private bool _mediaPollRunning;
    private bool _mediaBroken;
    private int _mediaFailures;

    private readonly Marquee _line1Scroll = new();
    private readonly Marquee _line2Scroll = new();

    public void RegisterVariables(VariableRegistry registry, string dev)
    {
        registry.Register($"{dev}_oled_clock", true, "OLED: clock (HH:mm with day and date)");
        registry.Register($"{dev}_oled_clock_icon", true, "OLED: clock icon");
        registry.Register($"{dev}_oled_clock_periodic", true, "OLED: show the clock periodically while music plays");
        registry.Register($"{dev}_oled_volume", true, "OLED: volume slider on volume changes");
        registry.Register($"{dev}_oled_song", true, "OLED: song information while music plays");
        registry.Register($"{dev}_oled_song_icon", true, "OLED: song icon");
        registry.Register($"{dev}_oled_song_flip", false, "OLED: artist above title");
        registry.Register($"{dev}_oled_tick_ms", 250, "OLED: scroll tick (ms)");
        registry.Register($"{dev}_oled_scroll_hold_ms", 2000, "OLED: pause at the start and end of a scroll (ms)");
    }

    public void Initialize(string dev)
    {
        var vars = Global.DeviceConfig.VarRegistry;
        _clock = vars.GetVariable<bool>($"{dev}_oled_clock");
        _clockIcon = vars.GetVariable<bool>($"{dev}_oled_clock_icon");
        _clockPeriodic = vars.GetVariable<bool>($"{dev}_oled_clock_periodic");
        _volume = vars.GetVariable<bool>($"{dev}_oled_volume");
        _song = vars.GetVariable<bool>($"{dev}_oled_song");
        _songIcon = vars.GetVariable<bool>($"{dev}_oled_song_icon");
        _songFlip = vars.GetVariable<bool>($"{dev}_oled_song_flip");
        _tickMs = Math.Max(50, vars.GetVariable<int>($"{dev}_oled_tick_ms"));
        _scrollHoldMs = Math.Max(0, vars.GetVariable<int>($"{dev}_oled_scroll_hold_ms"));
        _lastFrame = [];
        _lastVol = -1;
        _sendErrorLogged = false;
        _drawErrorLogged = false;
        _mediaBroken = false;
        _mediaFailures = 0;
        _line1Scroll.Reset();
        _line2Scroll.Reset();
    }

    public void Tick(HidStream stream, int reportLength, Action<string, Exception> logError)
    {
        var now = DateTime.Now;
        PollVolume(now);
        PollMedia(now);

        var frame = new byte[reportLength];
        frame[1] = Command;

        try
        {
            if (_volume && now < _volShownUntil)
                DrawVolumeScreen(frame);
            else if (_song && _playing && !(InPeriodicClockWindow(now) && _clock))
                DrawSongScreen(frame, now);
            else if (_clock)
                DrawClockScreen(frame, now);
        }
        catch (Exception e)
        {
            // Tick runs at the head of the device update; a drawing fault must not take the
            // colour frame down with it. Drop this frame and leave the panel on the last one.
            if (!_drawErrorLogged)
            {
                logError("failed to draw OLED frame", e);
                _drawErrorLogged = true;
            }
            return;
        }

        if (_lastFrame.AsSpan().SequenceEqual(frame))
            return;

        try
        {
            stream.SetFeature(frame);
            _lastFrame = frame;
            _sendErrorLogged = false;
        }
        catch (Exception e)
        {
            if (!_sendErrorLogged)
            {
                logError("failed to write OLED frame", e);
                _sendErrorLogged = true;
            }
            _lastFrame = frame; // do not retry the same frame every tick
        }
    }

    private bool InPeriodicClockWindow(DateTime now) =>
        _clockPeriodic && now.Second % PeriodicClockPeriodS >= PeriodicClockPeriodS - 5;

    private void PollVolume(DateTime now)
    {
        if (!_volume || _volBroken)
            return;

        try
        {
            _coreAudio ??= new CoreAudioVolume();
            var (vol, mute) = _coreAudio.Get();
            if (_lastVol >= 0 && (Math.Abs(vol - _lastVol) > 0.001f || mute != _lastMute))
                _volShownUntil = now.AddMilliseconds(VolumeOverlayMs);
            _lastVol = vol;
            _lastMute = mute;
        }
        catch
        {
            // audio endpoint may be switching; retry from scratch, give up only on repeated failure
            _coreAudio?.Dispose();
            _coreAudio = null;
            if (++_volFailures > 5)
                _volBroken = true;
        }
    }

    private int _volFailures;

    private void PollMedia(DateTime now)
    {
        if (!_song || _mediaBroken || _mediaPollRunning || (now - _lastMediaPoll).TotalMilliseconds < MediaPollMs)
            return;

        _lastMediaPoll = now;
        _mediaPollRunning = true;
        _ = Task.Run(async () =>
        {
            try
            {
                _smtc ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

                // GetCurrentSession() follows whatever Windows focused last, which is happily a
                // paused Spotify while something else plays. Honour it when it is really playing,
                // otherwise take the first session that is.
                var current = _smtc.GetCurrentSession();
                var session = IsPlaying(current) ? current : _smtc.GetSessions().FirstOrDefault(IsPlaying) ?? current;
                _mediaFailures = 0;

                if (session == null)
                {
                    _playing = false;
                    return;
                }

                _playing = IsPlaying(session);
                var props = await session.TryGetMediaPropertiesAsync();
                _title = Sanitize(props.Title ?? "");
                _artist = Sanitize(props.Artist ?? "");
            }
            catch
            {
                // SMTC can fail while the session manager is still coming up or a player exits
                // mid-call; rebuild it and give up only once it keeps failing.
                _smtc = null;
                if (++_mediaFailures > 5)
                    _mediaBroken = true;
            }
            finally
            {
                _mediaPollRunning = false;
            }
        });
    }

    private static bool IsPlaying(GlobalSystemMediaTransportControlsSession? session)
    {
        try
        {
            return session?.GetPlaybackInfo()?.PlaybackStatus ==
                   GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        }
        catch
        {
            return false; // session torn down between enumerating and asking
        }
    }

    // The 5x7 font is ASCII; fold what folds (e-acute -> e, curly quotes -> ') so foreign titles stay
    // readable. Whatever is left renders as '?' in DrawText rather than a silent blank.
    private static string Sanitize(string text)
    {
        if (text.All(c => c < 0x80))
            return text;

        try
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                if (Transliterations.TryGetValue(c, out var replacement))
                    sb.Append(replacement);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
        catch
        {
            return text; // malformed surrogates; DrawText falls back to '?' per character
        }
    }

    // Scroll position is state, not a function of the wall clock, and the sweep stops at both ends.
    // A wrap-around marquee never stands still: it flicks from the tail straight back into the head and
    // keeps going, which is unreadable on a 128px panel. This holds on the first window, steps to the
    // last full window, holds there, then snaps back to the start and holds again.
    // A clock-derived offset also dropped every new title halfway in (0 of 20 track changes started at
    // the first character) and kept running behind the periodic clock window, skipping 21 characters
    // across one 5s window - wider than the viewport, so that text never reached the panel at all.
    private sealed class Marquee
    {
        private string _text = "";
        private int _offset;
        private DateTime _nextAdvance = DateTime.MinValue;

        public void Reset() => _text = "";

        public int Advance(string text, int maxChars, DateTime now, int tickMs, int holdMs)
        {
            var last = Math.Max(0, text.Length - maxChars);

            if (!string.Equals(_text, text, StringComparison.Ordinal))
            {
                _text = text;
                _offset = 0;
                _nextAdvance = now.AddMilliseconds(holdMs);
                return 0;
            }

            if (now < _nextAdvance)
                return _offset;

            if (_offset >= last)
            {
                _offset = 0; // end reached and held; back to the start for another hold
                _nextAdvance = now.AddMilliseconds(holdMs);
                return _offset;
            }

            _offset++;
            // Schedule from now, so a stretch where this line was not drawn (clock window, volume
            // overlay) pauses the marquee instead of catching up over text nobody saw.
            _nextAdvance = now.AddMilliseconds(_offset >= last ? holdMs : tickMs);
            return _offset;
        }
    }

    private void DrawVolumeScreen(byte[] frame)
    {
        var pct = (int)Math.Round(_lastVol * 100);
        var label = _lastMute ? "MUTE" : $"{pct}%";
        DrawText(frame, (Width - label.Length * 6) / 2, 4, label);

        // bar with 1px border, fill follows volume (empty when muted)
        const int bx = 8, by = 20, bw = 112, bh = 12;
        FillRect(frame, bx, by, bw, 1);
        FillRect(frame, bx, by + bh - 1, bw, 1);
        FillRect(frame, bx, by, 1, bh);
        FillRect(frame, bx + bw - 1, by, 1, bh);
        if (!_lastMute)
            FillRect(frame, bx + 2, by + 2, (int)((bw - 4) * _lastVol), bh - 4);
    }

    private void DrawSongScreen(byte[] frame, DateTime now)
    {
        var line1 = _songFlip ? _artist : _title;
        var line2 = _songFlip ? _title : _artist;

        var x1 = 2;
        if (_songIcon)
        {
            DrawIcon(frame, 2, 8, NoteIcon);
            x1 = 14;
        }

        DrawScrollingText(frame, x1, 8, Width - x1 - 2, line1, now, _line1Scroll);
        DrawScrollingText(frame, 2, 24, Width - 4, line2, now, _line2Scroll);
    }

    private void DrawScrollingText(byte[] frame, int x, int y, int avail, string text, DateTime now, Marquee marquee)
    {
        var maxChars = avail / 6;
        if (maxChars <= 0)
            return;
        if (text.Length <= maxChars)
        {
            DrawText(frame, x, y, text);
            return;
        }

        var offset = marquee.Advance(text, maxChars, now, _tickMs, _scrollHoldMs);
        DrawText(frame, x, y, text.Substring(offset, maxChars));
    }

    private void DrawClockScreen(byte[] frame, DateTime now)
    {
        if (_clockIcon)
            DrawIcon(frame, 2, 2, ClockIcon);

        const int dw = 15, dh = 26, t = 3, top = 1;

        void DrawDigit(int x, int d)
        {
            var s = SevenSegment[d];
            if ((s & 1) != 0) FillRect(frame, x, top, dw, t);
            if ((s & 2) != 0) FillRect(frame, x + dw - t, top, t, dh / 2);
            if ((s & 4) != 0) FillRect(frame, x + dw - t, top + dh / 2, t, dh / 2);
            if ((s & 8) != 0) FillRect(frame, x, top + dh - t, dw, t);
            if ((s & 16) != 0) FillRect(frame, x, top + dh / 2, t, dh / 2);
            if ((s & 32) != 0) FillRect(frame, x, top, t, dh / 2);
            if ((s & 64) != 0) FillRect(frame, x, top + dh / 2 - t / 2, dw, t);
        }

        var text = now.ToString("HHmm");
        ReadOnlySpan<int> xs = [22, 43, 70, 91];
        for (var i = 0; i < 4; i++)
            DrawDigit(xs[i], text[i] - '0');
        FillRect(frame, 62, top + 6, t, t);
        FillRect(frame, 62, top + dh - 9, t, t);

        var date = $"{now:ddd} {now:d}".ToUpperInvariant();
        DrawText(frame, (Width - date.Length * 6) / 2, 32, date);
    }

    private static void FillRect(byte[] frame, int x, int y, int w, int h)
    {
        for (var yy = y; yy < y + h; yy++)
        for (var xx = x; xx < x + w; xx++)
        {
            if ((uint)xx >= Width || (uint)yy >= Height) continue;
            frame[2 + (yy >> 3) * Width + xx] |= (byte)(1 << (yy & 7));
        }
    }

    private static void DrawText(byte[] frame, int x, int y, string text)
    {
        foreach (var raw in text)
        {
            if (SmallFont.TryGetValue(raw, out var glyph) ||
                SmallFont.TryGetValue(char.ToUpperInvariant(raw), out glyph) ||
                SmallFont.TryGetValue('?', out glyph))
                for (var c = 0; c < 5; c++)
                for (var b = 0; b < 7; b++)
                    if ((glyph[c] & (1 << b)) != 0)
                        FillRect(frame, x + c, y + b, 1, 1);
            x += 6;
            if (x >= Width) return;
        }
    }

    private static void DrawIcon(byte[] frame, int x, int y, byte[] icon)
    {
        for (var c = 0; c < icon.Length; c++)
        for (var b = 0; b < 8; b++)
            if ((icon[c] & (1 << b)) != 0)
                FillRect(frame, x + c, y + b, 1, 1);
    }

    // 8x8 icons, column bytes, bit 0 = top
    private static readonly byte[] ClockIcon = [0x3C, 0x42, 0x81, 0x8F, 0x89, 0x81, 0x42, 0x3C];
    private static readonly byte[] NoteIcon = [0x60, 0xF0, 0xF0, 0x7F, 0x01, 0x02, 0x0C, 0x00];

    // segment bits: 1=top 2=right-top 4=right-bottom 8=bottom 16=left-bottom 32=left-top 64=middle
    private static readonly byte[] SevenSegment = [0b0111111, 0b0000110, 0b1011011, 0b1001111, 0b1100110, 0b1101101, 0b1111101, 0b0000111, 0b1111111, 0b1101111];

    // Characters FormD does not decompose, plus the typography players like to put in titles.
    private static readonly Dictionary<char, string> Transliterations = new()
    {
        ['\u00A0'] = " ", ['\u2007'] = " ", ['\u202F'] = " ",
        ['\u2018'] = "'", ['\u2019'] = "'", ['\u201A'] = ",", ['\u2032'] = "'",
        ['\u201C'] = "\"", ['\u201D'] = "\"", ['\u201E'] = "\"", ['\u2033'] = "\"",
        ['\u2010'] = "-", ['\u2011'] = "-", ['\u2012'] = "-", ['\u2013'] = "-", ['\u2014'] = "-", ['\u2015'] = "-",
        ['\u2026'] = "...",
        ['\u00D7'] = "x", ['\u00B7'] = ".", ['\u2022'] = ".",
        ['\u00DF'] = "ss", ['\u00E6'] = "ae", ['\u00C6'] = "AE", ['\u0153'] = "oe", ['\u0152'] = "OE",
        ['\u00F8'] = "o", ['\u00D8'] = "O", ['\u0142'] = "l", ['\u0141'] = "L",
        ['\u00F0'] = "d", ['\u00D0'] = "D", ['\u00FE'] = "th", ['\u00DE'] = "Th",
        ['\u0111'] = "d", ['\u0110'] = "D", ['\u0131'] = "i",
    };

    // 5x7 column font (bit 0 = top)
    private static readonly Dictionary<char, byte[]> SmallFont = new()
    {
        [' '] = [0x00, 0x00, 0x00, 0x00, 0x00],
        ['0'] = [0x3E, 0x51, 0x49, 0x45, 0x3E],
        ['1'] = [0x00, 0x42, 0x7F, 0x40, 0x00],
        ['2'] = [0x42, 0x61, 0x51, 0x49, 0x46],
        ['3'] = [0x21, 0x41, 0x45, 0x4B, 0x31],
        ['4'] = [0x18, 0x14, 0x12, 0x7F, 0x10],
        ['5'] = [0x27, 0x45, 0x45, 0x45, 0x39],
        ['6'] = [0x3C, 0x4A, 0x49, 0x49, 0x30],
        ['7'] = [0x01, 0x71, 0x09, 0x05, 0x03],
        ['8'] = [0x36, 0x49, 0x49, 0x49, 0x36],
        ['9'] = [0x06, 0x49, 0x49, 0x29, 0x1E],
        ['A'] = [0x7E, 0x11, 0x11, 0x11, 0x7E],
        ['B'] = [0x7F, 0x49, 0x49, 0x49, 0x36],
        ['C'] = [0x3E, 0x41, 0x41, 0x41, 0x22],
        ['D'] = [0x7F, 0x41, 0x41, 0x22, 0x1C],
        ['E'] = [0x7F, 0x49, 0x49, 0x49, 0x41],
        ['F'] = [0x7F, 0x09, 0x09, 0x09, 0x01],
        ['G'] = [0x3E, 0x41, 0x49, 0x49, 0x7A],
        ['H'] = [0x7F, 0x08, 0x08, 0x08, 0x7F],
        ['I'] = [0x00, 0x41, 0x7F, 0x41, 0x00],
        ['J'] = [0x20, 0x40, 0x41, 0x3F, 0x01],
        ['K'] = [0x7F, 0x08, 0x14, 0x22, 0x41],
        ['L'] = [0x7F, 0x40, 0x40, 0x40, 0x40],
        ['M'] = [0x7F, 0x02, 0x0C, 0x02, 0x7F],
        ['N'] = [0x7F, 0x04, 0x08, 0x10, 0x7F],
        ['O'] = [0x3E, 0x41, 0x41, 0x41, 0x3E],
        ['P'] = [0x7F, 0x09, 0x09, 0x09, 0x06],
        ['Q'] = [0x3E, 0x41, 0x51, 0x21, 0x5E],
        ['R'] = [0x7F, 0x09, 0x19, 0x29, 0x46],
        ['S'] = [0x46, 0x49, 0x49, 0x49, 0x31],
        ['T'] = [0x01, 0x01, 0x7F, 0x01, 0x01],
        ['U'] = [0x3F, 0x40, 0x40, 0x40, 0x3F],
        ['V'] = [0x1F, 0x20, 0x40, 0x20, 0x1F],
        ['W'] = [0x3F, 0x40, 0x38, 0x40, 0x3F],
        ['X'] = [0x63, 0x14, 0x08, 0x14, 0x63],
        ['Y'] = [0x07, 0x08, 0x70, 0x08, 0x07],
        ['Z'] = [0x61, 0x51, 0x49, 0x45, 0x43],
        ['a'] = [0x20, 0x54, 0x54, 0x54, 0x78],
        ['b'] = [0x7F, 0x48, 0x44, 0x44, 0x38],
        ['c'] = [0x38, 0x44, 0x44, 0x44, 0x20],
        ['d'] = [0x38, 0x44, 0x44, 0x48, 0x7F],
        ['e'] = [0x38, 0x54, 0x54, 0x54, 0x18],
        ['f'] = [0x08, 0x7E, 0x09, 0x01, 0x02],
        ['g'] = [0x0C, 0x52, 0x52, 0x52, 0x3E],
        ['h'] = [0x7F, 0x08, 0x04, 0x04, 0x78],
        ['i'] = [0x00, 0x44, 0x7D, 0x40, 0x00],
        ['j'] = [0x20, 0x40, 0x44, 0x3D, 0x00],
        ['k'] = [0x7F, 0x10, 0x28, 0x44, 0x00],
        ['l'] = [0x00, 0x41, 0x7F, 0x40, 0x00],
        ['m'] = [0x7C, 0x04, 0x18, 0x04, 0x78],
        ['n'] = [0x7C, 0x08, 0x04, 0x04, 0x78],
        ['o'] = [0x38, 0x44, 0x44, 0x44, 0x38],
        ['p'] = [0x7C, 0x14, 0x14, 0x14, 0x08],
        ['q'] = [0x08, 0x14, 0x14, 0x18, 0x7C],
        ['r'] = [0x7C, 0x08, 0x04, 0x04, 0x08],
        ['s'] = [0x48, 0x54, 0x54, 0x54, 0x20],
        ['t'] = [0x04, 0x3F, 0x44, 0x40, 0x20],
        ['u'] = [0x3C, 0x40, 0x40, 0x20, 0x7C],
        ['v'] = [0x1C, 0x20, 0x40, 0x20, 0x1C],
        ['w'] = [0x3C, 0x40, 0x30, 0x40, 0x3C],
        ['x'] = [0x44, 0x28, 0x10, 0x28, 0x44],
        ['y'] = [0x0C, 0x50, 0x50, 0x50, 0x3C],
        ['z'] = [0x44, 0x64, 0x54, 0x4C, 0x44],
        ['-'] = [0x08, 0x08, 0x08, 0x08, 0x08],
        ['/'] = [0x20, 0x10, 0x08, 0x04, 0x02],
        ['.'] = [0x00, 0x60, 0x60, 0x00, 0x00],
        [','] = [0x00, 0x50, 0x30, 0x00, 0x00],
        ['\''] = [0x00, 0x05, 0x03, 0x00, 0x00],
        ['"'] = [0x00, 0x07, 0x00, 0x07, 0x00],
        ['!'] = [0x00, 0x00, 0x5F, 0x00, 0x00],
        ['?'] = [0x02, 0x01, 0x51, 0x09, 0x06],
        [':'] = [0x00, 0x36, 0x36, 0x00, 0x00],
        [';'] = [0x00, 0x56, 0x36, 0x00, 0x00],
        ['('] = [0x00, 0x1C, 0x22, 0x41, 0x00],
        [')'] = [0x00, 0x41, 0x22, 0x1C, 0x00],
        ['&'] = [0x36, 0x49, 0x55, 0x22, 0x50],
        ['+'] = [0x08, 0x08, 0x3E, 0x08, 0x08],
        ['='] = [0x14, 0x14, 0x14, 0x14, 0x14],
        ['_'] = [0x40, 0x40, 0x40, 0x40, 0x40],
        ['*'] = [0x14, 0x08, 0x3E, 0x08, 0x14],
        ['%'] = [0x23, 0x13, 0x08, 0x64, 0x62],
        ['#'] = [0x14, 0x7F, 0x14, 0x7F, 0x14],
        ['@'] = [0x32, 0x49, 0x79, 0x41, 0x3E],
        ['|'] = [0x00, 0x00, 0x7F, 0x00, 0x00],
    };
}

// Minimal CoreAudio interop to read the default render endpoint's master volume.
internal sealed class CoreAudioVolume : IDisposable
{
    private readonly IAudioEndpointVolume _volume;

    public CoreAudioVolume()
    {
        var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(0 /* eRender */, 1 /* eMultimedia */, out var device));
        var iid = typeof(IAudioEndpointVolume).GUID;
        Marshal.ThrowExceptionForHR(device.Activate(ref iid, 1 /* CLSCTX_INPROC_SERVER */, IntPtr.Zero, out var itf));
        _volume = (IAudioEndpointVolume)itf;
    }

    public (float Volume, bool Mute) Get()
    {
        Marshal.ThrowExceptionForHR(_volume.GetMasterVolumeLevelScalar(out var vol));
        Marshal.ThrowExceptionForHR(_volume.GetMute(out var mute));
        return (vol, mute);
    }

    public void Dispose()
    {
        // RCWs are released by the GC; nothing deterministic needed here.
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumerator;

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(int dataFlow, int stateMask, out IntPtr devices);
        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        int Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object itf);
    }

    [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IntPtr notify);
        int UnregisterControlChangeNotify(IntPtr notify);
        int GetChannelCount(out uint count);
        int SetMasterVolumeLevel(float level, ref Guid eventContext);
        int SetMasterVolumeLevelScalar(float level, ref Guid eventContext);
        int GetMasterVolumeLevel(out float level);
        int GetMasterVolumeLevelScalar(out float level);
        int SetChannelVolumeLevel(uint channel, float level, ref Guid eventContext);
        int SetChannelVolumeLevelScalar(uint channel, float level, ref Guid eventContext);
        int GetChannelVolumeLevel(uint channel, out float level);
        int GetChannelVolumeLevelScalar(uint channel, out float level);
        int SetMute(bool mute, ref Guid eventContext);
        int GetMute(out bool mute);
    }
}
