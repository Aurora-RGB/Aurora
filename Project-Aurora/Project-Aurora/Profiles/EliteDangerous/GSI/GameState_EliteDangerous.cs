using System;
using AuroraRgb.Profiles.EliteDangerous.GSI.Nodes;

namespace AuroraRgb.Profiles.EliteDangerous.GSI;

public class GameStateCondition(
    long flagsSet = Flag.UNSPECIFIED,
    long flagsNotSet = Flag.UNSPECIFIED,
    GuiFocus[]? guiFocus = null,
    Func<GameState_EliteDangerous, bool>? callback = null)
{
    public GameStateCondition(Func<GameState_EliteDangerous, bool> callback) : this(0, 0, null, callback)
    {
    }

    public bool IsSatisfied(GameState_EliteDangerous gameState)
    {
        if (callback != null && !callback(gameState))
        {
            return false;
        }

        if (guiFocus != null && Array.IndexOf(guiFocus, gameState.Status.GuiFocus) == Flag.UNSPECIFIED)
        {
            return false;
        }

        if (flagsSet != Flag.UNSPECIFIED && !Flag.IsFlagSet(gameState.Status.Flags, flagsSet))
        {
            return false;
        }

        if (flagsNotSet != Flag.UNSPECIFIED && Flag.AtLeastOneFlagSet(gameState.Status.Flags, flagsNotSet))
        {
            return false;
        }

        return true;
    }
}

public class NeedsGameState
{
    public GameStateCondition? NeededGameStateCondition;

    public NeedsGameState()
    {
    }

    public NeedsGameState(GameStateCondition neededGameStateCondition)
    {
        NeededGameStateCondition = neededGameStateCondition;
    }

    public bool IsSatisfied(GameState_EliteDangerous gameState)
    {
        return NeededGameStateCondition == null || NeededGameStateCondition.IsSatisfied(gameState);
    }
}

public partial class GameState_EliteDangerous : GameState
{
    private Status? _status;
    private Nodes.Journal? _journal;
    private Nodes.Controls? _controls;

    public Nodes.Journal Journal => _journal ??= new Nodes.Journal();

    public Status Status => _status ??= new Status();

    public Nodes.Controls Controls => _controls ??= new Nodes.Controls();

    /// <summary>
    /// Creates a default GameState_EliteDangerous instance.
    /// </summary>
    public GameState_EliteDangerous() : base()
    {
    }
}