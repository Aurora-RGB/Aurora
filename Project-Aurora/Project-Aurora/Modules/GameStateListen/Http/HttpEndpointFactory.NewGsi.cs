using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Controls;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static AuroraRegexEndpoint GsiEndpoint(AuroraHttpListener listener)
    {
        var methods = new Dictionary<string, Action<HttpContext, Match>>
        {
            ["POST"] = ProcessPostGsi
        };
        return new AuroraRegexEndpoint(methods, GameStateRegex());

        void ProcessPostGsi(HttpContext context, Match match)
        {
            if (Window_GSIHttpDebug.IsOpen)
            {
                ProcessGameStateJson(listener, context, match);
            }
            else
            {
                ProcessGameStateStream(listener, context, match);
            }
        }
    }

    private static void ProcessGameStateJson(AuroraHttpListener listener, HttpContext context, Match match)
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
            // also return NewtonsoftGameState for GSI debug window 
            var gameState = new NewtonsoftGameState(json, false);
            listener.CurrentGameState = gameState;
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "[NetworkListener] ReceiveGameState error");
            Global.logger.Debug("JSON: {Json}", json);
        }
    }

    private static void ProcessGameStateStream(AuroraHttpListener listener, HttpContext context, Match match)
    {
        var request = context.Request;
        var jsonStream = request.Body;

        // immediately respond to the game
        SetResponse(context);

        try
        {
            var gameIdGroup = match.Groups[1];
            var gameId = gameIdGroup.Value;

            listener.OnNewJsonGameState(gameId, jsonStream);
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "[NetworkListener] ReceiveGameState error");
        }
    }

    private static void SetResponse(HttpContext context)
    {
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentLength = 0;
        foreach (var (key, value) in WebHeaderCollection)
        {
            response.Headers[key] = value;
        }
    }

    // https://regex101.com/r/N3BMIu/1
    [GeneratedRegex(@"\/gameState\/([a-zA-Z0-9_]*)\??\/?.*")]
    private static partial Regex GameStateRegex();
}