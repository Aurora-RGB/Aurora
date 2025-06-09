using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator.NodeProperties.GeneratedClasses;

public static class PartialGameStateGenerator
{
    public static string GetSource(INamedTypeSymbol classSymbol, ImmutableList<PropertyAccess> properties)
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
                 {{string.Join(",\n", properties.Select(Selector()))}}
                          }.ToFrozenDictionary();
                          public override FrozenDictionary<string, Func<AuroraRgb.Profiles.IGameState, object?>> PropertyMap => _innerProperties;
                     }
                 }
                 """;
    }

    private static Func<PropertyAccess, string> Selector() => AccessorMethodSource;

    private static string AccessorMethodSource(PropertyAccess valueTuple)
    {
        var pathString = valueTuple.GsiPath;
        return $"[\"{pathString}\"]\t=\t(t) => {valueTuple.AccessPath}";
    }
}