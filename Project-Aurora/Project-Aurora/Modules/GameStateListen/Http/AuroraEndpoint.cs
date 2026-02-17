using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AuroraRgb.Modules.GameStateListen.Http;

public class AuroraEndpoint(Dictionary<string, Action<HttpListenerContext>> methods, string path) : IAuroraEndpoint
{
    public string Path { get; } = path;
    public string[] AvailableMethods { get; } = methods.Keys.ToArray();
    private readonly FrozenDictionary<string, Action<HttpListenerContext>> _methods = methods.ToFrozenDictionary();

    public bool HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;

        if (!_methods.TryGetValue(request.HttpMethod, out var handler)) return false;
        handler(context);
        return true;
    }
}