namespace AuroraRgb.Profiles.RocketLeague.GSI.Events;

public class RlMatchEnded(int winnerTeamNum)
{
    public int WinnerTeamNum { get; } = winnerTeamNum;
}