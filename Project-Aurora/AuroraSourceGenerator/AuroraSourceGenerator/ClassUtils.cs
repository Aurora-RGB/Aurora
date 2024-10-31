using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator;

public static class ClassUtils
{
    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol type)
    {
        var currentType = type;
        while (currentType != null)
        {
            yield return currentType;
            currentType = currentType.BaseType;
        }
    }

    public static IEnumerable<ITypeSymbol> GetBaseTypes(ITypeSymbol type)
    {
        var currentType = type;
        while (currentType != null)
        {
            yield return currentType;
            currentType = currentType.BaseType;
        }
    }

    public static IEnumerable<ITypeSymbol> GetAllInterfaces(ITypeSymbol type)
    {
        return type.AllInterfaces;
    }
}