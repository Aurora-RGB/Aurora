using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AuroraRgb.Profiles.RocketLeague;

public static partial class RlStatsInstallUtils
{

    public static async Task EnableRlSocket(string iniPath, int setPort)
    {
        var content = await File.ReadAllTextAsync(iniPath);
        const int packetSendRate = 30;

        var portReplacement = $"Port={setPort}";
        var packetSendRateReplacement = $"PacketSendRate={packetSendRate}";

        var updatedContent = PortKeyValRegex()
            .Replace(content, portReplacement);
        updatedContent = PacketSendRateKeyValRegex()
            .Replace(updatedContent, packetSendRateReplacement);

        await File.WriteAllTextAsync(iniPath, updatedContent);
    }
    
    [GeneratedRegex(@"Port\s*=\s*\d+")]
    private static partial Regex PortKeyValRegex();

    [GeneratedRegex(@"PacketSendRate\s*=\s*\d+")]
    private static partial Regex PacketSendRateKeyValRegex();
    
    
}