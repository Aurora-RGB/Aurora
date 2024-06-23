using AuroraRgb.Profiles.ETS2.GSI.Nodes;

namespace AuroraRgb.Profiles.ETS2.GSI;

public class GameState_ETS2 : NewtonsoftGameState {

    internal Box<ETS2MemoryStruct> _memdat;
    private GameNode? _Game;
    private TruckNode? _Truck;
    private TrailerNode? _Trailer;
    private JobNode? _Job;
    private NavigationNode? _Navigation;

    /// <summary>
    /// Information about the game and the telemetry server.
    /// </summary>
    public GameNode Game => _Game ??= new GameNode(_memdat);

    /// <summary>
    /// Information about the truck the player is driving.
    /// </summary>
    public TruckNode Truck => _Truck ??= new TruckNode(_memdat);

    /// <summary>
    /// Information about the trailer attached to the truck the player is driving.
    /// </summary>
    public TrailerNode Trailer => _Trailer ??= new TrailerNode(_memdat);

    /// <summary>
    /// Information about the job the player is contracted on.
    /// </summary>
    public JobNode Job => _Job ??= new JobNode(_memdat);

    /// <summary>
    /// Information about the current route navigator route.
    /// </summary>
    public NavigationNode Navigation => _Navigation ??= new NavigationNode(_memdat);

    /// <summary>
    /// Creates a default GameState_ETS2 instance.
    /// </summary>
    public GameState_ETS2()
    { }

    /// <summary>
    /// Creates a GameState_ETS2 instance based on the passed JSON data.
    /// </summary>
    /// <param name="jsonData">The JSON data to parse.</param>
    public GameState_ETS2(string jsonData) : base(jsonData) { }

    /// <summary>
    /// Creates a GameState_ETS2 instance based on data that has been read from the MemoryMappedFile
    /// into a ETS2MemoryStruct.
    /// </summary>
    /// <param name="memdat">The struct the MemoryMappedFile data has been copied into.</param>
    internal GameState_ETS2(ETS2MemoryStruct memdat)
    {
        _memdat = new Box<ETS2MemoryStruct> { value = memdat };
    }
}

/// <summary>
/// Class to allow the structure to be passed and stored as a reference instead of value
/// </summary>
public class Box<T> {
    public T value;
}