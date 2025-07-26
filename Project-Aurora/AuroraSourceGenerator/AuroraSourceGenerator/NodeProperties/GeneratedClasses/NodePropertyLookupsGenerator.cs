using System;
using System.Collections.Generic;
using System.Linq;

namespace AuroraSourceGenerator.NodeProperties.GeneratedClasses;

public static class NodePropertyLookupsGenerator
{
    public static string GetSource(Dictionary<string, List<PropertyLookupInfo>> lookups)
    {
        return $$"""
                 // Auto-generated code
                 // {{DateTime.Now}}
                 #nullable enable

                 using System;
                 using System.Collections.Generic;
                 using System.Collections.Frozen;
                 using System.CodeDom.Compiler;

                 namespace AuroraRgb.Nodes
                 {
                     public class NodePropertyLookups
                     {
                          [GeneratedCode("AuroraRGB", "1.0.0")]
                          private static readonly FrozenDictionary<Type, List<PropertyLookup>> _innerProperties = new Dictionary<Type, List<PropertyLookup>>()
                          {
                 {{string.Join(",\n", lookups.Select(DictSourceEntry()))}}
                          }.ToFrozenDictionary();
                          [GeneratedCode("AuroraRGB", "1.0.0")]
                          public static FrozenDictionary<Type, List<PropertyLookup>> PropertyMap => _innerProperties;
                     }
                     
                     [GeneratedCode("AuroraRGB", "1.0.0")]
                     public class PropertyLookup
                     {
                         public string Name { get; }
                         public string GsiPath { get; }
                         public bool IsFolder { get; }
                         public Type? Type { get; }
                         public string? Description { get; }
                         
                         public PropertyLookup(string name, string gsiPath, Type? type)
                         {
                             Name = name;
                             GsiPath = gsiPath;
                             IsFolder = false;
                             Type = type;
                         }
                         
                         public PropertyLookup(string name, string gsiPath, string? description)
                         {
                             Name = name;
                             GsiPath = gsiPath;
                             IsFolder = true;
                             Description = description;
                         }
                     }
                 }
                 """;
    }

    private static Func<KeyValuePair<string, List<PropertyLookupInfo>>, string> DictSourceEntry()
    {
        return kvp =>
        {
            var join = string.Join(",\n", kvp.Value.Select(PropertySourceLine));
            return
                $"[typeof({kvp.Key})] = new List<PropertyLookup>() {{\n{join}\n}}";
        };
    }

    private static string PropertySourceLine(PropertyLookupInfo p)
    {
        if (p.PropertyType == null)
        {
            return $"new PropertyLookup(\"{p.Name}\", \"{p.GsiPath}\", \"\"\"\n{p.Description}\n\"\"\")";
        }

        return $"new PropertyLookup(\"{p.Name}\", \"{p.GsiPath}\", typeof({p.PropertyType.ToDisplayString().TrimEnd('?')}))";
    }
}