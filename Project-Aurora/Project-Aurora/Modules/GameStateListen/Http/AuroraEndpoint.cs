using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Net;

namespace AuroraRgb.Modules.GameStateListen.Http;

public class AuroraEndpoint(Dictionary<string, Action<HttpListenerContext>> methods, string path)
{
    public string Path { get; } = path;
    private readonly FrozenDictionary<string, Action<HttpListenerContext>> _methods = methods.ToFrozenDictionary();

    public void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (_methods.TryGetValue(request.HttpMethod, out var handler))
        {
            handler(context);
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            response.Close();
        }
    }
}