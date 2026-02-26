using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static AuroraEndpoint FaviconMethod()
    {
        var methods = new Dictionary<string, Action<HttpContext>>
        {
            ["GET"] = ReturnFavicon
        };
        return new AuroraEndpoint(methods, "/favicon.ico");
        
        void ReturnFavicon(HttpContext context)
        {
            var favicon = Properties.Resources.aurora_icon_bytes;
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "image/x-icon";
            response.ContentLength = favicon.Length;
            response.Body.Write(favicon, 0, favicon.Length);
            foreach (var (key, value) in WebHeaderCollection)
            {
                response.Headers[key] = value;
            }
        }
    }
}