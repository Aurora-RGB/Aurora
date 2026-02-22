using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static readonly FrozenDictionary<string, string> OptionsHeaderCollection = new Dictionary<string, string>
    {
        ["Access-Control-Allow-Origin"] = "*",
        ["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS",
        ["Access-Control-Allow-Headers"] = "Content-Type",
        ["Access-Control-Allow-Private-Network"] = "true",
    }.ToFrozenDictionary();
    
    private static AuroraRegexEndpoint OptionsMethod()
    {
        var methods = new Dictionary<string, Action<HttpContext, Match>>
        {
            ["OPTIONS"] = ProcessOptions
        };
        return new AuroraRegexEndpoint(methods, AnyPathRegex());
        
        void ProcessOptions(HttpContext context, Match match)
        {
            try
            {
                var response = context.Response;
                response.StatusCode = (int)HttpStatusCode.OK;
                foreach (var keyValuePair in OptionsHeaderCollection)
                {
                    response.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "[NetworkListener] Options error on: {Path}", context.Request.Path);
            }
        }
    }

    [GeneratedRegex(@".*")]
    private static partial Regex AnyPathRegex();
}