using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AuroraRgb.Modules.Razer;

public readonly record struct ChromaGameEvent(string Game, string EventName);

/// <summary>
/// Standalone reader for SetEventName-based Chroma games (#292), WITHOUT Synapse/cloud.
/// SetEventName games (e.g. 007: First Light) forward each event into the Chromatic event ring buffer
/// Global\{280F4D44-...} as UTF-16LE records "v1;play;&lt;EventName&gt;;{...name:&lt;Game&gt;...}". This reader
/// polls the ring counter at offset 0x0 and, on each increment, parses the newest record (slot
/// (counter-1)%100 at 0x8 + slot*0x8008) and raises <see cref="EventReceived"/>.
/// </summary>
public sealed partial class ChromaEventReader : IDisposable
{
    private const string MappingName = @"Global\{280F4D44-6AC8-4B21-9F33-EFD0548D76B4}";
    private const int RecordBase = 0x8;
    private const int RecordStride = 0x8008;
    private const int SlotCount = 100;

    private const uint FileMapRead = 0x0004;

    [LibraryImport("kernel32", EntryPoint = "OpenFileMappingW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr OpenFileMappingW(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, string name);
    [LibraryImport("kernel32", SetLastError = true)]
    private static partial IntPtr MapViewOfFile(IntPtr h, uint access, uint offHigh, uint offLow, UIntPtr bytes);
    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnmapViewOfFile(IntPtr addr);
    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr h);

    public event EventHandler<ChromaGameEvent>? EventReceived;

    public string CurrentGame { get; private set; } = string.Empty;
    public string LastEvent { get; private set; } = string.Empty;

    /// <summary>The firing game's process image name (e.g. "007firstlight.exe"), resolved from the record PID.</summary>
    public string CurrentProcess { get; private set; } = string.Empty;

    /// <summary>The firing game's PID from the newest record; 0 when the records carry none.</summary>
    public int CurrentPid { get; private set; }

    private readonly System.Collections.Generic.Dictionary<int, string> _processByPid = new();

    private readonly Thread _thread;
    private volatile bool _running = true;
    private uint _lastCounter;

    public ChromaEventReader()
    {
        _thread = new Thread(PollLoop) { IsBackground = true, Name = "ChromaEventReader" };
        _thread.Start();
    }

    private void PollLoop()
    {
        while (_running)
        {
            try
            {
                ReadOnce();
            }
            catch
            {
                // never let the poll thread die on a transient read error
            }
            Thread.Sleep(60);
        }
    }

    private void ReadOnce()
    {
        var h = OpenFileMappingW(FileMapRead, false, MappingName);
        if (h == IntPtr.Zero) return; // buffer not present (no Chroma game running)
        try
        {
            var p = MapViewOfFile(h, FileMapRead, 0, 0, UIntPtr.Zero);
            if (p == IntPtr.Zero) return;
            try
            {
                var counter = (uint)Marshal.ReadInt32(p, 0);
                if (counter == 0 || counter == _lastCounter) return;

                // First poll: baseline only. Records already in the buffer predate this session
                // (a stale MainMenu_On from a closed game would otherwise replay on every start).
                if (_lastCounter == 0)
                {
                    _lastCounter = counter;
                    return;
                }

                // Catch up over any records added since the last poll (bounded to one ring of slots).
                var from = _lastCounter + 1;
                if (counter - from >= SlotCount) from = counter - (SlotCount - 1);
                for (var c = from; c <= counter; c++)
                {
                    var off = RecordBase + (int)((c - 1) % SlotCount) * RecordStride;
                    Dispatch(ReadUtf16(p, off));
                }
                _lastCounter = counter;
            }
            finally { UnmapViewOfFile(p); }
        }
        finally { CloseHandle(h); }
    }

    private void Dispatch(string record)
    {
        // record = "v1;<verb>;<arg>[;<json>]"
        var parts = record.Split(';');
        if (parts.Length < 3 || parts[1] != "play") return;
        var ev = parts[2];
        var game = ExtractName(record);
        if (game.Length > 0) CurrentGame = game;
        var pid = ExtractPid(record);
        if (pid > 0) CurrentPid = pid;
        var proc = ResolveProcess(pid);
        if (proc.Length > 0) CurrentProcess = proc;
        LastEvent = ev;
        EventReceived?.Invoke(this, new ChromaGameEvent(CurrentGame, ev));
    }

    private static string ReadUtf16(IntPtr basePtr, int offset)
    {
        var sb = new StringBuilder();
        for (var i = offset; ; i += 2)
        {
            var c = (char)(ushort)Marshal.ReadInt16(basePtr, i);
            if (c == 0 || sb.Length > 4096) break;
            sb.Append(c);
        }
        return sb.ToString();
    }

    private static string ExtractName(string record)
    {
        const string marker = "\"name\":\"";
        var i = record.IndexOf(marker, StringComparison.Ordinal);
        if (i < 0) return string.Empty;
        i += marker.Length;
        var j = record.IndexOf('"', i);
        return j < 0 ? string.Empty : record[i..j];
    }

    private static int ExtractPid(string record)
    {
        const string marker = "\"PID\":";
        var i = record.IndexOf(marker, StringComparison.Ordinal);
        if (i < 0) return 0;
        i += marker.Length;
        var j = i;
        while (j < record.Length && char.IsDigit(record[j])) j++;
        return int.TryParse(record[i..j], out var pid) ? pid : 0;
    }

    // Resolve a PID to its image name (e.g. "007firstlight.exe") so the profile can activate on the firing
    // game WITHOUT the Razer registry. Cached per PID; failures (process gone) cache an empty string.
    private string ResolveProcess(int pid)
    {
        if (pid <= 0) return string.Empty;
        if (_processByPid.TryGetValue(pid, out var cached)) return cached;
        string name;
        try { name = (System.Diagnostics.Process.GetProcessById(pid).ProcessName + ".exe").ToLowerInvariant(); }
        catch { name = string.Empty; }
        _processByPid[pid] = name;
        return name;
    }

    public void Dispose()
    {
        _running = false;
        _thread.Join(500);
    }
}
