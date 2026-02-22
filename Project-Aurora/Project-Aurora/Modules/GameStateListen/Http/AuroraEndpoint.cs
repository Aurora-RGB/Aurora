using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public class AuroraEndpoint(Dictionary<string, Action<HttpContext>> methods, string path) : IAuroraEndpoint
{
    public string Path { get; } = path;
    public string[] AvailableMethods { get; } = methods.Keys.ToArray();
    private readonly FrozenDictionary<string, Action<HttpContext>> _methods = methods.ToFrozenDictionary();

    public bool HandleRequest(HttpContext context)
    {
        var request = context.Request;

        if (!_methods.TryGetValue(request.Method, out var handler)) return false;
        handler(context);
        return true;
    }
}