using System.Windows.Forms;

namespace AuroraRgb.Nodes;

public class BatteryNode : Node
{
    public static BatteryChargeStatus ChargeStatus => SystemInformation.PowerStatus.BatteryChargeStatus;
    public static bool PluggedIn => SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Offline;
    public static float LifePercent => SystemInformation.PowerStatus.BatteryLifePercent;
    public static int SecondsRemaining => SystemInformation.PowerStatus.BatteryLifeRemaining;
}