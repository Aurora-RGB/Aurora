using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator.NodeProperties.GeneratedClasses;

public static class PartialGameStateGenerator
{
    public static string GetSource(INamedTypeSymbol classSymbol, ImmutableList<PropertyLookupInfo> properties)
    {
        return $$"""
                 // Auto-generated code
                 // {{DateTime.Now}}
                 #nullable enable

                 using System;
                 using System.Collections.Generic;
                 using System.Collections.Frozen;

                 namespace {{classSymbol.ContainingNamespace}}
                 {
                     public partial class {{classSymbol.Name}}
                     {
                         private static readonly FrozenDictionary<string, Func<AuroraRgb.Profiles.IGameState, object?>> _innerProperties = new Dictionary<string, Func<AuroraRgb.Profiles.IGameState, object?>>()
                          {
                 {{string.Join(",\n", properties.Where(Filter).Select(AccessorMethodSource))}}
                          }.ToFrozenDictionary();
                          public override FrozenDictionary<string, Func<AuroraRgb.Profiles.IGameState, object?>> PropertyMap => _innerProperties;
                     }
                 }
                 """;
    }

    private static bool Filter(PropertyLookupInfo arg)
    {
        return !arg.IsFolder;
    }

    private static string AccessorMethodSource(PropertyLookupInfo valueTuple)
    {
        var pathString = valueTuple.GsiPath;
        return $"[\"{pathString}\"]\t=\t(t) => {valueTuple.AccessPath}";
    }
}