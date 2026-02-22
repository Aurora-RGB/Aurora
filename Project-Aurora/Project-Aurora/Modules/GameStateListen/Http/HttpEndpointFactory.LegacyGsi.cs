using System;
using System.Collections.Generic;
using AuroraRgb.Profiles;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static AuroraEndpoint LegacyGsiEndpoint(AuroraHttpListener httpListener)
    {
        var methods = new Dictionary<string, Action<HttpContext>>
        {
            ["POST"] = ProcessPostGsi
        };
        return new AuroraEndpoint(methods, "/");
        
        void ProcessPostGsi(HttpContext context)
        {
            var json = ReadContent(context);
            try
            {
                var gameState = new NewtonsoftGameState(json);
                httpListener.CurrentGameState = gameState;
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "[NetworkListener] ReceiveGameState error on: {Path}", context.Request.Path);
                Global.logger.Debug("JSON: {Json}", json);
            }
        }
    }
}