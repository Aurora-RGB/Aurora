using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace AuroraRgb.Profiles.Minecraft;

public class MinecraftPlugin(string name, string downloadUrl, DateTimeOffset assetUpdatedAt)
{
    public string Name { get; } = name;
    public string DownloadUrl { get; } = downloadUrl;
    public DateTimeOffset AssetUpdatedAt { get; } = assetUpdatedAt;
}

public class MinecraftPlugins(string version, IEnumerable<MinecraftPlugin> plugins)
{
    public string Version { get; } = version;
    public IEnumerable<MinecraftPlugin> Plugins { get; } = plugins;

    public static async Task<MinecraftPlugins> GetPlugins()
    {
        // fetch latest https://github.com/Aurora-RGB/Minecraft-GSI/releases
        var githubClient = new GitHubClient(new ProductHeaderValue("aurora-minecraft"));
        var lastRelease = await githubClient.Repository.Release.GetLatest("Aurora-RGB", "Minecraft-GSI");

        var releases = lastRelease.Assets.Where(asset => asset.Name.EndsWith(".jar"))
            .OrderBy(asset => asset.Name)
            .Select(asset => new MinecraftPlugin(asset.Name, asset.BrowserDownloadUrl, asset.UpdatedAt));
        return new MinecraftPlugins(lastRelease.Name, releases);
    }
}