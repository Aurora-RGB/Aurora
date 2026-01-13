using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace AuroraDeviceManager.Utils;

public static class ProcessUtils
{
    public static bool IsProcessRunning(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return false;
        }

        var normalizedName = Path.GetFileNameWithoutExtension(processName).Trim();
        if (string.IsNullOrEmpty(normalizedName))
        {
            normalizedName = processName.Trim();
        }

        try
        {
            var processesByName = Process.GetProcessesByName(normalizedName);
            if (processesByName.Length > 0)
            {
                return true;
            }
        }
        catch
        {
            // ignore and fall back to service lookup
        }

        try
        {
            using var service = new ServiceController(normalizedName);
            return service.Status is ServiceControllerStatus.Running
                   or ServiceControllerStatus.StartPending
                   or ServiceControllerStatus.ContinuePending;
        }
        catch (InvalidOperationException)
        {
            // service not found
        }
        catch (Win32Exception)
        {
            // access denied or service not accessible
        }

        return false;
    }
}