using System.IO;
using System.Net;
using System.Threading.Tasks;
using AuroraRgb.Properties;
using AuroraRgb.Utils.Steam;
using ICSharpCode.SharpZipLib.Zip;

namespace AuroraRgb.Profiles.Payday_2.GSI;

public static class Pd2GsiUtils
{
    /// <summary>
    /// Installs GSI mod for Payday 2
    /// </summary>
    /// <returns>error message if fails</returns>
    public static async Task<string?> InstallMod()
    {
        var pd2Path = await SteamUtils.GetGamePathAsync(218620);

        if (string.IsNullOrWhiteSpace(pd2Path))
        {
            return "Payday 2 is not installed through Steam.\r\nCould not install the GSI mod.";
        }

        if (!Directory.Exists(pd2Path))
        {
            return "Payday 2 directory is not found.\r\nCould not install the GSI mod.";
        }

        if (!Directory.Exists(Path.Combine(pd2Path, "mods")))
        {
            return "BLT Hook was not found.\r\nCould not install the GSI mod.";
        }

        //create GSI config folder, in case game is not run with GSI mod installed
        var gsiConfigDir = Path.Combine(pd2Path, "GSI");
        Directory.CreateDirectory(gsiConfigDir);

        //copy gsi config file
        using var gsiConfigFile = new MemoryStream(Resources.PD2_GSI);
        await File.WriteAllBytesAsync(Path.Combine(pd2Path, "GSI", "Aurora.xml"), gsiConfigFile.ToArray());

        // extract mod
        await ExtractPd2Mod(pd2Path);

        return null;
    }

    private static async Task ExtractPd2Mod(string pd2Path)
    {
        const string zipUrl = "https://github.com/Aurora-RGB/Payday2-GSI/archive/refs/heads/main.zip";
        var modFolder = Path.Combine(pd2Path, "mods", "GSI");
        using var webClient = new WebClient();
        using var zipStream = new MemoryStream(webClient.DownloadData(zipUrl));
        await using var zipInputStream = new ZipInputStream(zipStream);
        while (zipInputStream.GetNextEntry() is { } entry)
        {
            if (!entry.IsFile)
                continue;

            var entryName = entry.Name;
            var fullPath = Path.Combine(modFolder, entryName).Replace("\\Payday2-GSI-main", "");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await using var entryFileStream = File.Create(fullPath);
            await zipInputStream.CopyToAsync(entryFileStream);
        }
    }
}