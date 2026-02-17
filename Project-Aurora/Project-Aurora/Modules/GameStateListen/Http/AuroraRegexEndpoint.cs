using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AuroraRgb.Modules.GameStateListen.Http;

public class AuroraRegexEndpoint(Dictionary<string, Action<HttpListenerContext, Match>> methods, Regex path) : IAuroraEndpoint
{
    public Regex Path { get; } = path;
    public string[] AvailableMethods { get; } = methods.Keys.ToArray();
    private readonly FrozenDictionary<string, Action<HttpListenerContext, Match>> _methods = methods.ToFrozenDictionary();

    /**
     * Processes the request if the method is supported,
     * otherwise returns false to indicate that the request should be handled by another endpoint
     */
    public bool HandleRequest(HttpListenerContext context, Match regexMatch)
    {
        var request = context.Request;

        if (!_methods.TryGetValue(request.HttpMethod, out var handler))
            return false;

        handler(context, regexMatch);
        return true;
    }
}