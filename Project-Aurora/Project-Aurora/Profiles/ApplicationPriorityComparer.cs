using System.Collections.Generic;

namespace AuroraRgb.Profiles;

public sealed class ApplicationPriorityComparer : IComparer<Application>
{
    public static readonly ApplicationPriorityComparer Instance = new();

    private ApplicationPriorityComparer()
    {
    }

    public int Compare(Application? x, Application? y)
    {
        // Then compare by Priority
        return Comparer<int?>.Default.Compare(y?.Config.Priority, x?.Config.Priority);
    }
}