using System.Collections.Frozen;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static readonly WebHeaderCollection WebHeaderCollection = new()
    {
        ["Access-Control-Allow-Origin"] = "*",
        ["Access-Control-Allow-Private-Network"] = "true",
    };

    public static FrozenDictionary<string, AuroraEndpoint> CreateEndpoints(AuroraHttpListener listener)
    {
        return ((AuroraEndpoint[])
            [
                VariablesEndpoint(),
                LegacyGsiEndpoint(listener),
                ProfilesEndpoint(),
            ])
            .ToFrozenDictionary(endpoint => endpoint.Path, endpoint => endpoint);
    }

    public static FrozenDictionary<Regex, AuroraRegexEndpoint> CreateRegexEndpoints(AuroraHttpListener listener)
    {
        return ((AuroraRegexEndpoint[])
            [
                GsiEndpoint(listener),
                OptionsMethod(),
            ])
            .ToFrozenDictionary(endpoint => endpoint.Path, endpoint => endpoint);
    }

    private static string ReadContent(HttpListenerContext context)
    {
        var request = context.Request;
        string json;

        using (var sr = new StreamReader(request.InputStream))
        {
            json = sr.ReadToEnd();
        }

        // immediately respond to the game, don't let it wait for response
        var response = context.Response;
        CloseConnection(response);

        return json;
    }

    private static void CloseConnection(HttpListenerResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentLength64 = 0;
        response.Headers = WebHeaderCollection;
        response.Close([], true);
    }
}