using System.ComponentModel;

namespace AuroraRgb.Modules.Updates;

public class AuroraChangelog(string versionTag, string content)
{
    [DesignOnly(true)]
    public string VersionTag => versionTag;

    [DesignOnly(true)]
    public string Content => content;
}