using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using AuroraRgb.Profiles;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static readonly WebHeaderCollection OptionsHeaderCollection = new()
    {
        ["Access-Control-Allow-Origin"] = "*",
        ["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS",
        ["Access-Control-Allow-Headers"] = "Content-Type",
        ["Access-Control-Allow-Private-Network"] = "true",
    };
    
    private static AuroraRegexEndpoint OptionsMethod()
    {
        var methods = new Dictionary<string, Action<HttpListenerContext, Match>>
        {
            ["OPTIONS"] = ProcessOptions
        };
        return new AuroraRegexEndpoint(methods, AnyPathRegex());
        
        void ProcessOptions(HttpListenerContext context, Match match)
        {
            try
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Headers = OptionsHeaderCollection;
                context.Response.Close([], true);
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "[NetworkListener] Options error on: {Path}", context.Request.Url?.LocalPath);
            }
        }
    }

    [GeneratedRegex(@".*")]
    private static partial Regex AnyPathRegex();
}