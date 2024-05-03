using System;
using System.Collections.Generic;

namespace AuroraRgb.Modules.Razer;

public readonly struct RzSdkVersion(int major, int minor, int revision) : IComparable<RzSdkVersion>
{
    private int Major { get; } = major;
    private int Minor { get; } = minor;
    private int Revision { get; } = revision;

    public override bool Equals(object? obj) => obj is RzSdkVersion ver && CompareTo(ver) == 0;

    public override string ToString() => $"{Major}.{Minor}.{Revision}";

    public override int GetHashCode()
    {
        var hashCode = -327234472;
        hashCode = hashCode * -1521134295 + Major.GetHashCode();
        hashCode = hashCode * -1521134295 + Minor.GetHashCode();
        hashCode = hashCode * -1521134295 + Revision.GetHashCode();
        return hashCode;
    }

    public int CompareTo(RzSdkVersion other)
    {
        var comparer = Comparer<int>.Default;

        int result;
        if ((result = comparer.Compare(Major, other.Major)) == 0 &&
            (result = comparer.Compare(Minor, other.Minor)) == 0)
            return comparer.Compare(Revision, other.Revision);

        return result;
    }

    public static bool operator ==(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) == 0;
    public static bool operator !=(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) != 0;
    public static bool operator >(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) > 0;
    public static bool operator <(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) < 0;
    public static bool operator >=(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) >= 0;
    public static bool operator <=(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) <= 0;
}