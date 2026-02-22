using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuroraRgb.Profiles;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static AuroraEndpoint ProfilesEndpoint()
    {
        Dictionary<string, Action<HttpContext>> methods = new()
        {
            ["GET"] = ProcessGetProfiles,
        };
        return new AuroraEndpoint(methods, "/profiles");
    }

    private static void ProcessGetProfiles(HttpContext context)
    {
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "application/json";
        foreach (var (key, value) in WebHeaderCollection)
        {
            response.Headers[key] = value;
        }

        var activeProfile = ConvertProfile(LightingStateManagerModule.LightningStateManager.Result.ApplicationManager.GetCurrentProfile());
        var activeOverlays = LightingStateManagerModule.LightningStateManager.Result.ApplicationManager.GetOverlayActiveProfiles()
            .Select(ConvertProfile);
        var responseJson = new ProfilesResponse(activeProfile, activeOverlays);

        using var sw = new StreamWriter(response.Body);
        JsonSerializer.Serialize(sw.BaseStream, responseJson, ProfilesJsonContext.Default.ProfilesResponse);

        ProfileResponse ConvertProfile(Application profile)
        {
            return new ProfileResponse(profile.Config.Name, profile.Config.ProcessNames, profile.Config.ProcessTitles?.Select(r => r.ToString()));
        }
    }
}

public class ProfilesResponse(ProfileResponse activeProfile, IEnumerable<ProfileResponse> activeOverlays)
{
    public ProfileResponse ActiveProfile { get; } = activeProfile;
    public IEnumerable<ProfileResponse> ActiveOverlays { get; } = activeOverlays;
}
    
public class ProfileResponse(string name, IEnumerable<string> processes, IEnumerable<string>? titles)
{
    public string Name { get; } = name;
    public IEnumerable<string> Processes { get; } = processes;
    public IEnumerable<string> Titles { get; } = titles ?? [];
}

[JsonSerializable(typeof(ProfilesResponse))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class ProfilesJsonContext : JsonSerializerContext;