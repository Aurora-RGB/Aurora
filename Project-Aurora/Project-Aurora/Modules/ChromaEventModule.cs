using System.Threading.Tasks;
using AuroraRgb.Modules.Razer;

namespace AuroraRgb.Modules;

/// <summary>
/// Standalone interception of SetEventName Chroma games (#292): reads the event stream from shared
/// memory (no Synapse/cloud) via <see cref="ChromaEventReader"/>. P1: log received events.
/// </summary>
public sealed class ChromaEventModule : AuroraModule
{
    private static readonly TaskCompletionSource<ChromaEventReader> ReaderSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static Task<ChromaEventReader> Reader => ReaderSource.Task;

    // Unique build/version stamp — bump on every rebuild so the log proves exactly which binary runs.
    public const string BuildTag = "CHROMAEVENT-BUILD-0016";

    private ChromaEventReader? _reader;

    protected override Task Initialize()
    {
        Global.logger.Information("Loading ChromaEventReader (standalone SetEventName interception) [{BuildTag}]", BuildTag);
        _reader = new ChromaEventReader();
        _reader.EventReceived += (_, e) =>
            Global.logger.Information("[ChromaEvent] game='{Game}' event='{Event}'", e.Game, e.EventName);
        ReaderSource.SetResult(_reader);
        return Task.CompletedTask;
    }

    public override ValueTask DisposeAsync()
    {
        _reader?.Dispose();
        _reader = null;
        return ValueTask.CompletedTask;
    }
}
