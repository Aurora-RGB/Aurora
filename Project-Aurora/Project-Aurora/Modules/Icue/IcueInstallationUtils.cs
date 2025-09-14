using System.Collections.Frozen;
using AuroraRgb.Utils;

namespace AuroraRgb.Modules.Icue;

public static class IcueInstallationUtils
{
    public const string IcueExe = "icue.exe";

    private static readonly FrozenSet<string> IcueAutoStartNames = new[]
    {
        "Corsair iCUE5 Software",
    }.ToFrozenSet();

    public static bool IsIcueInstalled()
    {
        return AutoStartUtils.IsSoftwareInstalled(IcueAutoStartNames);
    }

    public static bool IsIcueAutorunEnabled()
    {
        return AutoStartUtils.IsAutorunEnabled(IcueAutoStartNames);
    }
}