using System.Text.RegularExpressions;
using Version = SemanticVersioning.Version;

namespace Aurora_Updater;

// https://regex101.com/r/jQ255t/1
public static partial class VersionParser
{
    public static Version ParseVersion(string versionString)
    {
        var regex = SemanticVersionRegex();
        var match = regex.Match(versionString);

        var groupCollection = match.Groups;

        int.TryParse(groupCollection[1].Value, out var major);
        int.TryParse(groupCollection[2].Value, out var minor);
        int.TryParse(groupCollection[3].Value, out var patch);

        var suffix = groupCollection[4].Success ? groupCollection[4].Value : null;
        if (string.IsNullOrWhiteSpace(suffix))
        {
            suffix = null;
        }

        return new Version(major, minor, patch, suffix);
    }

    [GeneratedRegex(@"v?(\d+)\.?(\d+)?\.?(\d+)?-?(.*)?")]
    private static partial Regex SemanticVersionRegex();
}