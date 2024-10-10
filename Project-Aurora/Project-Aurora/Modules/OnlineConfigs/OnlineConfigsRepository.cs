using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AuroraRgb.Modules.OnlineConfigs.Model;
using AuroraRgb.Utils.Json;

namespace AuroraRgb.Modules.OnlineConfigs;

[JsonSerializable(typeof(OnlineSettingsMeta))]
[JsonSerializable(typeof(DeviceTooltips))]
[JsonSerializable(typeof(ConflictingProcesses))]
[JsonSerializable(typeof(Dictionary<string, DeviceTooltips>))]
[JsonSerializable(typeof(RazerDevices))]
internal partial class OnlineSettingsSourceGenerationContext : JsonSerializerContext;

public static class OnlineConfigsRepository
{
    private const string ConflictingProcesses = "ConflictingProcesses.json";
    private const string DeviceTooltips = "DeviceInformations.json";
    private const string OnlineSettings = "OnlineSettings.json";
    private const string RazerDevices = "RazerDevices.json";

    private static readonly string ConflictingProcessLocalCache = Path.Combine(".", ConflictingProcesses);
    private static readonly string DeviceTooltipsLocalCache = Path.Combine(".", DeviceTooltips);
    private static readonly string OnlineSettingsLocalCache = Path.Combine(".", OnlineSettings);
    private static readonly string RazerDeviceInfoLocalCache = Path.Combine(".", RazerDevices);

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new DateTimeOffsetConverterUsingDateTimeParse() },
        TypeInfoResolverChain = { OnlineSettingsSourceGenerationContext.Default }
    };

    public static async Task<ConflictingProcesses> GetConflictingProcesses()
    {
        return await ParseLocalJson<ConflictingProcesses>(ConflictingProcessLocalCache);
    }

    public static async Task<Dictionary<string, DeviceTooltips>> GetDeviceTooltips()
    {
        return await ParseLocalJson<Dictionary<string, DeviceTooltips>>(DeviceTooltipsLocalCache);
    }

    public static async Task<OnlineSettingsMeta> GetOnlineSettingsLocal()
    {
        return await ParseLocalJson<OnlineSettingsMeta>(OnlineSettingsLocalCache);
    }

    public static async Task<RazerDevices> GetRazerDeviceInfo()
    {
        return await ParseLocalJson<RazerDevices>(RazerDeviceInfoLocalCache);
    }

    public static async Task<OnlineSettingsMeta> GetOnlineSettingsOnline()
    {
        var stream = await ReadOnlineJson(OnlineSettings);

        return await JsonSerializer.DeserializeAsync<OnlineSettingsMeta>(stream, JsonSerializerOptions) ?? new OnlineSettingsMeta();
    }

    private static async Task<T> ParseLocalJson<T>(string cachePath) where T : new()
    {
        await using var stream = GetJsonStream(cachePath);

        try
        {
            var deserialize = await JsonSerializer.DeserializeAsync<T>(stream, JsonSerializerOptions);
            return deserialize ?? new T();
        }
        catch (Exception e)
        {
            Global.logger.Error($"Error parsing local json: {cachePath}", e);
        }
        return new T();
    }

    private static Stream GetJsonStream(string cachePath)
    {
        return File.Exists(cachePath) ? File.Open(cachePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) : new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
    }

    private static async Task<Stream> ReadOnlineJson(string file)
    {
        var url = "https://raw.githubusercontent.com/Aurora-RGB/Online-Settings/master/" + file;
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync();
        }

        //internet check is done before but...
        throw new ApplicationException("Internet access is lost during update");
    }
}