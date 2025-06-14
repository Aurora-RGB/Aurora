﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator.NodeProperties;

public static class PropertyAnalyzer
{
    public static IEnumerable<PropertyAccess> GetClassProperties(string currentPath, INamedTypeSymbol type, string upperAccessPath)
    {
        var namedTypeSymbols = ClassUtils.GetBaseTypes(type);
        foreach (var currentType in namedTypeSymbols)
        {
            var classAccessors = GetClassAccessors(currentPath, currentType, upperAccessPath);
            foreach (var propertyAccess in classAccessors) yield return propertyAccess;
        }
    }

    private static IEnumerable<PropertyAccess> GetClassAccessors(string currentPath, INamedTypeSymbol currentType, string upperAccessPath)
    {
        foreach (var member in currentType.GetMembers())
        {
            if (member is not IPropertySymbol property) continue;
            if (!PropertyAccessible(property)) continue;
            if (property.Name == "Parent") continue;
            if (property.IsIndexer) continue;
            if (GenericAttribute(property)) continue; // skip generic properties
            if (IsPropertyIgnored(property)) continue;

            var propertyType = property.Type;

            var accessPath = property.IsStatic ? GetStaticAccessPath(currentType, property) : upperAccessPath + property.Name;

            if (IsPropertyFinal(propertyType))
            {
                yield return new PropertyAccess(currentPath + property.Name, accessPath.TrimEnd('.'), propertyType);
                continue;
            }

            if (propertyType.TypeKind is not (TypeKind.Class or TypeKind.Interface)) continue;

            var namedTypeSymbol = (INamedTypeSymbol)property.Type;
            if (!IsAuroraClass(namedTypeSymbol))
                continue;

            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol, currentType))
            {
                continue;
            }

            var s = property.NullableAnnotation == NullableAnnotation.Annotated ? "?." : ".";
            var lowerAccessPath = property.IsStatic ? accessPath : accessPath + s;

            var folderPath = currentPath + property.Name + "/";

            foreach (var classProperty in GetClassProperties(folderPath, namedTypeSymbol, lowerAccessPath))
            {
                yield return classProperty;
            }
        }
    }


    private static bool IsPropertyFinal(ITypeSymbol propertyType)
        => propertyType.IsValueType || propertyType.SpecialType == SpecialType.System_String;

    private static bool PropertyAccessible(IPropertySymbol property)
        => property.GetMethod?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;

    private static bool GenericAttribute(IPropertySymbol property)
        => property.Name.Contains('<');

    private static bool IsPropertyIgnored(IPropertySymbol property)
        => property.GetAttributes().Any(IgnoredAttribute);

    private static bool IgnoredAttribute(AttributeData arg)
        => arg.AttributeClass?.Name == "GameStateIgnoreAttribute";

    private static string GetStaticAccessPath(INamedTypeSymbol currentType, IPropertySymbol property)
        => currentType.ContainingNamespace + "." + currentType.Name + "." + property.Name + ".";
    private static bool IsAuroraClass(INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ToString().StartsWith("AuroraRgb.");

}