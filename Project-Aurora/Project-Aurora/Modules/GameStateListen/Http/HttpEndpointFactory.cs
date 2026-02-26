using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static readonly FrozenDictionary<string, string> WebHeaderCollection = new Dictionary<string, string>
    {
        ["Access-Control-Allow-Origin"] = "*",
        ["Access-Control-Allow-Private-Network"] = "true",
    }.ToFrozenDictionary();

    public static FrozenDictionary<string, AuroraEndpoint> CreateEndpoints(AuroraHttpListener listener)
    {
        return ((AuroraEndpoint[])
            [
                VariablesEndpoint(),
                LegacyGsiEndpoint(listener),
                ProfilesEndpoint(),
                FaviconMethod(),
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

    private static string ReadContent(HttpContext context)
    {
        var request = context.Request;
        string json;

        using (var sr = new StreamReader(request.Body))
        {
            json = sr.ReadToEnd();
        }

        // immediately respond to the game, don't let it wait for response
        var response = context.Response;
        CloseConnection(response);

        return json;
    }

    private static void CloseConnection(HttpResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentLength = 0;
        foreach (var (key, value) in WebHeaderCollection)
        {
            response.Headers[key] = value;
        }
    }
}