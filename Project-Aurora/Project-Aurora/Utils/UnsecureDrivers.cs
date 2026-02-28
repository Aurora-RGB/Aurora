using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace AuroraRgb.Utils;

public static class UnsecureDrivers
{
    public const string InpOutDriverName = "inpoutx64";
    public const string WinRing0DriverName = "WinRing0x64";
    private const string ServicesPath = @"SYSTEM\CurrentControlSet\Services\";

    public static bool WinRing0DriverExists()
    {
        using var r = Registry.LocalMachine.OpenSubKey(ServicesPath + WinRing0DriverName);
        return r != null;
    }
    
    public static bool InpOutDriverExists()
    {
        using var r = Registry.LocalMachine.OpenSubKey(ServicesPath + InpOutDriverName);
        return r != null;
    }

    public static void DeleteDriver(string driverName, bool silent = false)
    {
        var inpOutKey = ServicesPath + driverName;

        try
        {
            using var r = Registry.LocalMachine.OpenSubKey(inpOutKey);
            if (r == null)
            {
                Global.logger.Information("Cannot delete driver: {DriverName} as it doesn't exist", driverName);
                return;
            }

            var imagePath = r.GetValue("ImagePath");
            var inpOutSysPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + Path.PathSeparator + imagePath;

            Registry.LocalMachine.DeleteSubKeyTree(inpOutKey);

            try
            {
                if (File.Exists(inpOutSysPath))
                {
                    File.Delete(inpOutSysPath);
                }

                ShowMessage("Deleted driver. Your system needs restart to fully take effect", "Success", MessageBoxImage.Information, silent);
            }
            catch(Exception e)
            {
                Global.logger.Error(e, "{DriverName}.sys could not be deleted", driverName);
                ShowMessage(driverName + ".sys could not be deleted. Restart your system and delete this file manually:\n" + inpOutSysPath, "Error", MessageBoxImage.Error, silent);
            }
        }
        catch(Exception e)
        {
            Global.logger.Error(e, "Could not delete {DriverName} registry keys", driverName);
        }
    }
    
    private static void ShowMessage(string message, string caption, MessageBoxImage icon, bool silent)
    {
        if (!silent)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, icon);
        }
        else
        {
            Global.logger.Error("Unsecure driver removal failed.\n{Message}", message);
        }
    }
}