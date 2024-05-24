using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace AuroraRgb.Utils;

/// <summary>
/// http://peterkellyonline.blogspot.com/2011/04/configuring-windows-service.html
/// </summary>
public static partial class ServiceHelper
{
    [LibraryImport(Advapi32Dll, EntryPoint = "ChangeServiceConfigW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ChangeServiceConfig(
        IntPtr hService,
        uint nServiceType,
        uint nStartType,
        uint nErrorControl,
        string? lpBinaryPathName,
        string? lpLoadOrderGroup,
        IntPtr lpdwTagId,
        [In] char[]? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword,
        string? lpDisplayName);

    [LibraryImport(Advapi32Dll, EntryPoint = "OpenServiceW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr OpenService(IntPtr hScManager, string lpServiceName, uint dwDesiredAccess);

    [LibraryImport(Advapi32Dll, EntryPoint = "OpenSCManagerW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr OpenScManager(
        string? machineName, string? databaseName, uint dwAccess);

    [LibraryImport(Advapi32Dll, EntryPoint = "CloseServiceHandle")]
    private static partial void CloseServiceHandle(IntPtr hScObject);

    private const uint ServiceNoChange = 0xFFFFFFFF;
    private const uint ServiceQueryConfig = 0x00000001;
    private const uint ServiceChangeConfig = 0x00000002;
    private const uint ScManagerAllAccess = 0x000F003F;
    private const string Advapi32Dll = "advapi32.dll";

    public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
    {
        var scManagerHandle = OpenScManager(null, null, ScManagerAllAccess);
        if (scManagerHandle == IntPtr.Zero)
        {
            throw new ExternalException("Open Service Manager Error");
        }

        var serviceHandle = OpenService(
            scManagerHandle,
            svc.ServiceName,
            ServiceQueryConfig | ServiceChangeConfig);

        if (serviceHandle == IntPtr.Zero)
        {
            throw new ExternalException("Open Service Error");
        }

        var result = ChangeServiceConfig(
            serviceHandle,
            ServiceNoChange,
            (uint)mode,
            ServiceNoChange,
            null,
            null,
            IntPtr.Zero,
            null,
            null,
            null,
            null);

        if (!result)
        {
            var nError = Marshal.GetLastWin32Error();
            var win32Exception = new Win32Exception(nError);
            throw new ExternalException("Could not change service start type: "
                                        + win32Exception.Message);
        }

        CloseServiceHandle(serviceHandle);
        CloseServiceHandle(scManagerHandle);
    }
}