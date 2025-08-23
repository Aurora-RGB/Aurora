using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    public static bool TryGetParentSyntax(SyntaxNode? syntaxNode, out BaseNamespaceDeclarationSyntax? result)
    {
        while (true)
        {
            // set defaults
            result = null;

            syntaxNode = syntaxNode?.Parent;

            switch (syntaxNode)
            {
                case null:
                    return false;
                case BaseNamespaceDeclarationSyntax bns:
                    result = bns;
                    return true;
                default:
                    continue;
            }
        }
    }
}