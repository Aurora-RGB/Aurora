namespace AuroraRgb.Nodes;

public class NETInfo : Node
{
    public static float Usage => LocalPcInformation.HardwareMonitor.Net.BandwidthUsed;
    public static float UploadSpeed => LocalPcInformation.HardwareMonitor.Net.UploadSpeedBytes;
    public static float DownloadSpeed => LocalPcInformation.HardwareMonitor.Net.DownloadSpeedBytes;
}