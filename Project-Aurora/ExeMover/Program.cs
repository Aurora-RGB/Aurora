using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Management.Automation;

const int csidlCommonStartmenu = 0x16; // All Users\Start Menu
const int csidlStartmenu = 0xb;

var globalStartup = new StringBuilder(260);
SHGetSpecialFolderPath(IntPtr.Zero, globalStartup, csidlCommonStartmenu, false);

var userStartup = new StringBuilder(260);
SHGetSpecialFolderPath(IntPtr.Zero, userStartup, csidlStartmenu, false);

string[] startPaths =
[
    Path.Combine(globalStartup.ToString(), "Programs"),
    Path.Combine(userStartup.ToString(), "Programs")
];

string[] shortcutNames = ["Aurora.lnk", "Aurora - Shortcut.lnk", "Aurora.exe - Shortcut.lnk"];

var auroraShortcutPaths = (from startPath in startPaths
    from shortcutName in shortcutNames
    select Path.Combine(startPath, shortcutName)).ToList();

var currentPath = Path.GetDirectoryName(Environment.ProcessPath);
if (currentPath == null)
{
    Process.Start("AuroraRgb.exe");
    return;
}

const string script = """
                      param (
                          [Parameter(Mandatory = $true)]
                          [string]$TargetPath,
                      
                          [Parameter(Mandatory = $true)]
                          [string[]]$LnkFiles
                      )
                      
                      Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted

                      foreach ($lnkFile in $LnkFiles) {
                          # Create a Shell object
                          $shell = New-Object -ComObject WScript.Shell
                      
                          # Check if the file exists
                          if (Test-Path $lnkFile) {
                              # Get the shortcut object
                              $shortcut = $shell.CreateShortcut($lnkFile)
                      
                              # Change the target path
                              $shortcut.TargetPath = $TargetPath
                      
                              # Save the changes
                              $shortcut.Save()
                      
                              Write-Output "Changed target for $($lnkFile) to $TargetPath"
                          } else {
                              Write-Output "File $($lnkFile) does not exist."
                          }
                      }

                      Write-Output "All .lnk files have been updated to target $TargetPath"
                      """;

var newPath = Path.Combine(currentPath, "AuroraRgb.exe");

var powershell = PowerShell.Create().AddScript(script);

powershell.Streams.Verbose.DataAdded += (_, eventArgs) =>
{
    Console.WriteLine(powershell.Streams.Verbose[eventArgs.Index]);
};
powershell.Streams.Debug.DataAdded += (_, eventArgs) =>
{
    Console.WriteLine(powershell.Streams.Debug[eventArgs.Index]);
};
powershell.Streams.Information.DataAdded += (_, eventArgs) =>
{
    Console.WriteLine(powershell.Streams.Information[eventArgs.Index]);
};
powershell.Streams.Warning.DataAdded += (_, eventArgs) =>
{
    Console.Error.WriteLine(powershell.Streams.Warning[eventArgs.Index]);
};
powershell.Streams.Error.DataAdded += (_, eventArgs) =>
{
    Console.Error.WriteLine(powershell.Streams.Error[eventArgs.Index]);
};

powershell.AddParameter("TargetPath", newPath);
powershell.AddParameter("LnkFiles", auroraShortcutPaths);
var psObjects = powershell.Invoke();
foreach (var psObject in psObjects)
{
    Console.WriteLine(psObject);
}

Process.Start("AuroraRgb.exe");
return;

[DllImport("shell32.dll")]
static extern bool SHGetSpecialFolderPath(IntPtr hndOwner,
    [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
