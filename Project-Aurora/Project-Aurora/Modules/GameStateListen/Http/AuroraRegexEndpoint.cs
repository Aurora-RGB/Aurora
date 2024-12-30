using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace AuroraRgb.Modules.GameStateListen.Http;

public class AuroraRegexEndpoint(Dictionary<string, Action<HttpListenerContext, Match>> methods, Regex path)
{
    public Regex Path { get; } = path;
    private readonly FrozenDictionary<string, Action<HttpListenerContext, Match>> _methods = methods.ToFrozenDictionary();

    public void HandleRequest(HttpListenerContext context, Match regexMatch)
    {
        var request = context.Request;
        var response = context.Response;

        if (_methods.TryGetValue(request.HttpMethod, out var handler))
        {
            handler(context, regexMatch);
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            response.Close();
        }
    }
}