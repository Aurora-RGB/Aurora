using AuroraRgb.Utils;

namespace AuroraRgb.Nodes;

public class TimeNode : Node
{
    public static int CurrentMonth => Time.GetMonths();
    public static int CurrentDay => Time.GetDays();
    public static int CurrentHour => Time.GetHours();
    public static int CurrentMinute => Time.GetMinutes();
    public static int CurrentSecond => Time.GetSeconds();
    public static int CurrentMillisecond => Time.GetMilliSeconds();
    public static long MillisecondsSinceEpoch => Time.GetMillisecondsSinceEpoch();
}