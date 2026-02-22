using System;
using System.Collections.Generic;
using System.IO;
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
            var inputStream = request.Body;
            // low mem-alloc process
            var jsonNode = JsonSerializer.Deserialize<JsonElement>(inputStream);

            // immediately respond, don't let it wait for response
            CloseConnection(context.Response);

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
                                var stringValue = value.GetString();
                                if (stringValue == null)
                                {
                                    AuroraVariables.Instance.Strings.Remove(key);
                                }
                                else
                                {
                                    AuroraVariables.Instance.Strings[key] = stringValue;
                                }

                                break;
                            case JsonValueKind.Number:
                                AuroraVariables.Instance.Numbers[key] = value.GetDouble();
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                AuroraVariables.Instance.Booleans[key] = value.GetBoolean();
                                break;
                            case JsonValueKind.Null:
                                AuroraVariables.Instance.Strings.Remove(key);
                                AuroraVariables.Instance.Numbers.Remove(key);
                                AuroraVariables.Instance.Booleans.Remove(key);
                                break;
                        }
                    }

                    break;
            }
        }
    }
}