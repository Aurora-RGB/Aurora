using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using AuroraRgb.Profiles;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static AuroraRegexEndpoint GsiEndpoint(AuroraHttpListener listener)
    {
        var methods = new Dictionary<string, Action<HttpListenerContext, Match>>
        {
            ["POST"] = ProcessPostGsi
        };
        return new AuroraRegexEndpoint(methods, GameStateRegex());

        void ProcessPostGsi(HttpListenerContext context, Match match)
        {
            var json = ReadContent(context);
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }

                var gameIdGroup = match.Groups[1];
                var gameId = gameIdGroup.Value;

                listener.OnNewJsonGameState(gameId, json);

                // set announce false to prevent LSM from setting it to a profile
                // also return NewtonsoftGameState for compatibility and GSI window 
                var gameState = new NewtonsoftGameState(json, false);
                listener.CurrentGameState = gameState;
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "[NetworkListener] ReceiveGameState error");
                Global.logger.Debug("JSON: {Json}", json);
            }
        }
    }

    // https://regex101.com/r/N3BMIu/1
    [GeneratedRegex(@"\/gameState\/([a-zA-Z0-9_]*)\??\/?.*")]
    private static partial Regex GameStateRegex();
}