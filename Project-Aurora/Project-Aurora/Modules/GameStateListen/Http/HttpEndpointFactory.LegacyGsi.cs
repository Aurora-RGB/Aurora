using System;
using System.Collections.Generic;
using System.Net;
using AuroraRgb.Profiles;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static AuroraEndpoint LegacyGsiEndpoint(AuroraHttpListener httpListener)
    {
        var methods = new Dictionary<string, Action<HttpListenerContext>>
        {
            ["POST"] = ProcessPostGsi
        };
        return new AuroraEndpoint(methods, "/");
        
        void ProcessPostGsi(HttpListenerContext context)
        {
            var json = ReadContent(context);
            try
            {
                var gameState = new NewtonsoftGameState(json);
                httpListener.CurrentGameState = gameState;
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "[NetworkListener] ReceiveGameState error on: {Path}", context.Request.Url!.LocalPath);
                Global.logger.Debug("JSON: {Json}", json);
            }
        }
    }
}