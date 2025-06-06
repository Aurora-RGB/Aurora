using System.Collections.Generic;

namespace AuroraRgb.Profiles;

public class ApplicationPriorityComparer : IComparer<Application>
{
    public int Compare(Application? x, Application? y)
    {
        if (x == null && y == null)
            return 0;
        if (x == null)
            return 1;
        if (y == null)
            return -1;

        // Compare by EnableByDefault first
        if (x.Config.EnableByDefault && !y.Config.EnableByDefault)
            return -1;
        if (!x.Config.EnableByDefault && y.Config.EnableByDefault)
            return 1;

        // Then compare by Priority
        return Comparer<int>.Default.Compare(y.Config.Priority, x.Config.Priority);
    }
}