using System;
using System.Text.Json.Serialization;

namespace AuroraRgb.Modules.Razer.RazerApi;

public class RazerInstallerManifest(RazerManifestInfo latest, string baseUrl)
{
    private static readonly Uri RazerAppsBaseUrl = new("https://apps.razer.com");
    public static Uri GetUrl = new(RazerAppsBaseUrl, "/chroma-app/dashboard/installer-manifest.json");
    
    [JsonPropertyName("baseURL")]
    public string BaseUrl { get; set; } = baseUrl;
    public RazerManifestInfo Latest { get; set; } = latest;

    public Uri LatestManifestAbsoluteUrl { get; } = new(new Uri(RazerAppsBaseUrl, baseUrl + "/"), latest.Url);
}

public class RazerManifestInfo(string version, string url)
{
    public string Version { get; set; } = version;
    public string Url { get; set; } = url;
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public class RazerManifest(RazerManifestResource[] resources)
{
    public RazerManifestResource[] Resources { get; set; } = resources;
}

public class RazerManifestResource(string resourceName, string url, string sha256, string integrity, bool restartRequired, RazerResourceAction action)
{
    public string ResourceName { get; set; } = resourceName;
    public string Url { get; set; } = url;
    public string Sha256 { get; set; } = sha256;
    public string Integrity { get; set; } = integrity;
    public bool RestartRequired { get; set; } = restartRequired;
    public RazerResourceAction Action { get; set; } = action;
}

public class RazerResourceAction(RazerResourceSaveAction saveToDisk)
{
    public RazerResourceSaveAction SaveToDisk { get; set; } = saveToDisk;
}


public class RazerResourceSaveAction(string? runOnInstall)
{
    public string? RunOnInstall { get; set; } = runOnInstall;
}
