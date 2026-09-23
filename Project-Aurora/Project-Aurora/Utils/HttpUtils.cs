using System.Net.Http;

namespace AuroraRgb.Utils;

public static class HttpUtils
{
    public static HttpClient HttpClient { get; } = new();
}