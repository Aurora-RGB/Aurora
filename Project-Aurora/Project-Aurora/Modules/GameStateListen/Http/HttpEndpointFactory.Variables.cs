using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using AuroraRgb.Nodes;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen.Http;

public static partial class HttpEndpointFactory
{
    private static AuroraEndpoint VariablesEndpoint()
    {
        var methods = new Dictionary<string, Action<HttpContext>>
        {
            ["GET"] = ProcessGetVariables,
            ["POST"] = ProcessPostVariables
        };
        return new AuroraEndpoint(methods, "/variables");

        void ProcessGetVariables(HttpContext context)
        {
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";
            foreach (var (key, value) in WebHeaderCollection)
            {
                response.Headers[key] = value;
            }

            JsonSerializer.Serialize<AuroraVariables>(response.Body, AuroraVariables.Instance, VariablesSourceGenContext.Default.AuroraVariables);
        }

        void ProcessPostVariables(HttpContext context)
        {
            var request = context.Request;
            if (request.Query.Count > 0)
            {
                ProcessQueryString(context);
            }
            
            var contentType = request.ContentType;
            if (contentType == null || contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                if (request.ContentLength is > 2)
                {
                    ProcessJsonInput(request, context);
                }
            }
            else if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                ProcessBodyFormData(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
    }

    private static void ProcessJsonInput(HttpRequest request, HttpContext context)
    {
        var inputStream = request.Body;
        // low mem-alloc process
        var jsonNode = JsonSerializer.Deserialize<JsonElement>(inputStream);

        // immediately respond, don't let it wait for response
        CloseConnection(context.Response);

        var auroraVariables = AuroraVariables.Instance;
        switch (jsonNode.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in jsonNode.EnumerateObject())
                {
                    var key = property.Name;
                    var value = property.Value;
                    switch (value.ValueKind)
                    {
                        case JsonValueKind.String:
                            var stringValue = value.GetString()!; 
                            auroraVariables.Strings[key] = stringValue;
                            break;
                        case JsonValueKind.Number:
                            auroraVariables.Numbers[key] = value.GetDouble();
                            break;
                        case JsonValueKind.True:
                            auroraVariables.Booleans[key] = true;
                            break;
                        case JsonValueKind.False:
                            auroraVariables.Booleans[key] = false;
                            break;
                        case JsonValueKind.Null:
                            auroraVariables.Strings.Remove(key);
                            auroraVariables.Numbers.Remove(key);
                            auroraVariables.Booleans.Remove(key);
                            break;
                    }
                }

                break;
        }
    }

    private static void ProcessBodyFormData(HttpContext context)
    {
        var request = context.Request;
        var form = request.Form;
        var auroraVariables = AuroraVariables.Instance;
        foreach (var (key, value) in form)
        {
            // try parse as number
            if (double.TryParse(value, out var numberValue))
            {
                auroraVariables.Numbers[key] = numberValue;
            }
            else if (bool.TryParse(value, out var boolValue))
            {
                auroraVariables.Booleans[key] = boolValue;
            }
            else
            {
                auroraVariables.Strings[key] = value;
            }
        }
    }
    
    private static void ProcessQueryString(HttpContext context)
    {
        var request = context.Request;
        var query = request.Query;
        var auroraVariables = AuroraVariables.Instance;
        foreach (var (key, value) in query)
        {
            // try parse as number
            if (double.TryParse(value, out var numberValue))
            {
                auroraVariables.Numbers[key] = numberValue;
            }
            else if (bool.TryParse(value, out var boolValue))
            {
                auroraVariables.Booleans[key] = boolValue;
            }
            else
            {
                auroraVariables.Strings[key] = value;
            }
        }
    }
}