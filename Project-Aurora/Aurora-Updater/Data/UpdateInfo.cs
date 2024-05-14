using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using SemanticVersioning;

namespace Aurora_Updater.Data;

public class UpdateInfo(Version currentVersion, string author, string repoName, bool getPreReleases)
{
    private readonly GitHubClient _gClient = new(new ProductHeaderValue("aurora-updater", currentVersion.ToString()));

    public IEnumerable<Release> FetchMissingReleases()
    {
        return EnumeratePages()
            .ToBlockingEnumerable()
            .TakeWhile(IsNewerVersion);
    }

    public async Task<bool> IsCurrentlyPreRelease()
    {
        // let's reduce API calls for development builds :)
        if (IsDevelopmentBuild())
        {
            return true;
        }
        if (getPreReleases)
        {
            return true;
        }

        try
        {
            var release = await _gClient.Repository.Release.Get(author, repoName, $"v{currentVersion.Major}");
            return release.Prerelease;
        }
        catch
        {
            return false;
        }
    }

    private async IAsyncEnumerable<Release> EnumeratePages()
    {
        // we need to limit for dev builds as they have the lowest version, causing this to fetch all releases
        if (IsDevelopmentBuild())
        {
            var lastRelease = await _gClient.Repository.Release.GetAll(author, repoName, new ApiOptions { PageCount = 1, PageSize = 3 });
            foreach (var release in lastRelease)
            {
                yield return release;
            }
            yield break;
        }
        
        var page = 1;
        while (page < 6)  // fetch up to 5 pages
        {
            var releases = await GetPage(page);
            foreach (var release in releases.OrderByDescending(r => r.PublishedAt))
            {
                yield return release;
            }

            page++;
        }
    }

    private async Task<IEnumerable<Release>> GetPage(int page)
    {
        return await _gClient.Repository.Release.GetAll(author, repoName, new ApiOptions { PageCount = page, PageSize = 10 });
    }

    private bool IsNewerVersion(Release release)
    {
        return VersionParser.ParseVersion(release.TagName) > currentVersion;
    }

    private bool IsDevelopmentBuild()
    {
        return currentVersion is { Major: 0, Minor: 0 };
    }
}