using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AuroraRgb.Modules.Razer.RazerApi;
using AuroraRgb.Utils;
using Microsoft.Win32;

namespace AuroraRgb.Modules.Razer;

public enum RazerChromaInstallerExitCode
{
    Success = 0,
    InvalidState = 1,
    RestartRequired = 3010
}

public static class ChromaInstallationUtils
{
    private const string DevicesXml = "Devices.xml";
    private const string FileContent = """
                                       <?xml version="1.0" encoding="utf-8"?>
                                       <devices>
                                       </devices>
                                       """;

    private const string RazerChromaSdkServer = "Razer Chroma SDK Server";
    private const string RazerChromaStreamServer = "Razer Chroma Stream Server";

    public static async Task<int> UninstallAsync() => await Task.Run(() =>
    {
        if (RzHelper.GetSdkVersion() == new RzSdkVersion())
            return (int)RazerChromaInstallerExitCode.InvalidState;

        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var key = hklm.OpenSubKey(@"Software\Razer\Synapse3\PID0302MW");
        if (key != null)
        {
            var filepath = (string)key.GetValue("UninstallPath", null);
            key.Close();

            var exitCode = DoUninstall(filepath);
            if (exitCode == (int)RazerChromaInstallerExitCode.RestartRequired)
                return exitCode;
        }

        key = hklm.OpenSubKey(@"Software\Razer\Synapse3\RazerChromaBroadcaster");
        if (key != null)
        {
            var filepath = (string)key.GetValue("UninstallerPath", null);
            key.Close();

            var exitCode = DoUninstall(filepath);
            if (exitCode == (int)RazerChromaInstallerExitCode.RestartRequired)
                return exitCode;
        }

        key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
        if (key == null) return (int)RazerChromaInstallerExitCode.Success;
        {
            var path = (string)key.GetValue("UninstallPath", null);
            var filename = (string)key.GetValue("UninstallFilename", null);
            key.Close();

            var exitCode = DoUninstall($@"{path}\{filename}");
            if (exitCode == (int)RazerChromaInstallerExitCode.RestartRequired)
                return exitCode;
        }

        return (int)RazerChromaInstallerExitCode.Success;

        int DoUninstall(string filepath)
        {
            var filename = Path.GetFileName(filepath);
            var path = Path.GetDirectoryName(filepath);
            var processInfo = new ProcessStartInfo
            {
                FileName = filename,
                WorkingDirectory = path,
                UseShellExecute = true,
                Arguments = $"/S _?={path}",
                ErrorDialog = true
            };

            var process = Process.Start(processInfo);
            process.WaitForExit(120000);
            return process.ExitCode;
        }
    });

    private static async Task<string?> GetDownloadUrlAsync()
    {
        const string endpoint = "prod";

        using var client = new HttpClient();
        var json = await client.GetStringAsync("https://discovery.razerapi.com/user/endpoints");
        var razerEndpoints = JsonSerializer.Deserialize(json, RazerApiSourceGenerationContext.Default.RazerEndpoints);
        var hash = razerEndpoints?.Endpoints.Find(c => c.Name == endpoint)?.Hash;

        if (hash == null)
            return null;

        const string platformData = """
                                    <PlatformRoot xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                                      <Platform>
                                        <Arch>64</Arch>
                                        <Locale>en</Locale>
                                        <Mfr>Generic-MFR</Mfr>
                                        <Model>Generic-MDL</Model>
                                        <OS>Windows</OS>
                                        <OSVer>10</OSVer>
                                        <SKU>Generic-SKU</SKU>
                                      </Platform>
                                    </PlatformRoot>
                                    """;

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://manifest.razerapi.com/api/legacy/{hash}/{endpoint}/productlist/get")
        {
            Content = new StringContent(platformData)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        var response = await client.SendAsync(request);
        var xml = await response.Content.ReadAsStringAsync();
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        foreach (XmlNode node in doc.DocumentElement.SelectNodes("//Module"))
            if (node["Name"].InnerText == "CHROMABROADCASTER")
                return node["DownloadURL"].InnerText;

        return null;
    }

    public static async Task<string?> DownloadAsync()
    {
        var url = await GetDownloadUrlAsync();
        if (url == null)
            return null;

        using var client = new HttpClient();
        await using var responseStream = await client.GetStreamAsync(url);

        var path = Path.ChangeExtension(Path.GetRandomFileName(), ".exe");
        await using var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        await responseStream.CopyToAsync(fileStream);

        return path;
    }

    public static async Task<int> InstallAsync(string installerPath)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = Path.GetFileName(installerPath),
            WorkingDirectory = Path.GetDirectoryName(installerPath),
            UseShellExecute = true,
            Arguments = "/S",
            ErrorDialog = true
        };

        var process = Process.Start(processInfo);
        await process.WaitForExitAsync(new CancellationTokenSource(120000).Token);
        return process.ExitCode;
    }

    public static void RestoreDeviceControl()
    {
        var chromaPath = GetChromaPath();
        if (chromaPath != null)
        {
            File.Delete(Path.Combine(chromaPath, DevicesXml));
        }

        var chromaPath64 = GetChromaPath64();
        if (chromaPath64 != null)
        {
            File.Delete(Path.Combine(chromaPath64, DevicesXml));
        }
        RestartChromaService();
    }
    

    public static async Task DisableDeviceControlAsync()
    {
        List<Task> tasks = [];
        ReplaceDevicesXml(tasks, GetChromaPath());
        ReplaceDevicesXml(tasks, GetChromaPath64());

        if (tasks.Count == 0)
        {
            return;
        }

        await Task.WhenAll(tasks.ToArray());

        RestartChromaService();
    }

    private static void ReplaceDevicesXml(List<Task> tasks, string? chromaPath)
    {
        if (chromaPath == null) return;

        var xmlFile = Path.Combine(chromaPath, DevicesXml);
        if (File.Exists(xmlFile))
        {
            var length = File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Length;
            if (length <= FileContent.Length)
            {
                return;
            }
        }
        tasks.Add(File.WriteAllTextAsync(xmlFile, FileContent));
    }

    public static void DisableChromaBloat()
    {
        DisableService(RazerChromaSdkServer);
        DisableService(RazerChromaStreamServer);
    }

    private static void DisableService(string serviceName)
    {
        using var service = new ServiceController(serviceName);
        try
        {
            ServiceHelper.ChangeStartMode(service, ServiceStartMode.Manual);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped);
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error disabling chroma service {ServiceName}", serviceName);
        }
    }

    private static void RestartChromaService()
    {
        using var service = new ServiceController("Razer Chroma SDK Service");
        if (service.Status == ServiceControllerStatus.Running)
        {
            try
            {
                service.Stop(true);
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            catch(Exception e)
            {
                Global.logger.Error(e, "Failed to stop chroma sdk");
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(2));
            }
        }

        if (service.Status != ServiceControllerStatus.Stopped) return;
        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running);
    }

    private static string? GetChromaPath()
    {
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
        return key?.GetValue("InstallPath", null) as string;
    }

    private static string? GetChromaPath64()
    {
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
        return key?.GetValue("InstallPath64", null) as string;
    }
}