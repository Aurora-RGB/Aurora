using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AuroraSourceGenerator;

internal class PropertyAccess(string gsiPath, string accessPath)
{
    public string GsiPath { get; } = gsiPath;
    public string AccessPath { get; } = accessPath;
}

/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator]
public class NodePropertySourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this example
    }

    public void Execute(GeneratorExecutionContext context)
    {
        const string gamestate = "AuroraRgb.Profiles.GameState";
        HashSet<string> ignore = [gamestate];
        
        var compilation = context.Compilation;

        // Retrieve the interface symbol
        var gameStateInterface = compilation.GetTypeByMetadataName(gamestate);

        if (gameStateInterface == null)
        {
            // Interface not found, do nothing
            return;
        }

        // Find all classes in the compilation
        var classes = compilation.SyntaxTrees
            .SelectMany(syntaxTree => syntaxTree.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>()
            .Select(declaration => ModelExtensions.GetDeclaredSymbol(compilation.GetSemanticModel(declaration.SyntaxTree), declaration))
            .OfType<INamedTypeSymbol>()
            .Where(IsAuroraClass)
            .Where(IsSubtypeOf(gameStateInterface))
            .Where(IsPartialClass)
            .Where(classSymbol => !ignore.Contains(classSymbol.ToDisplayString()));

        foreach (var classSymbol in classes)
        {
            GenerateClassProperties(context, classSymbol);
        }
    }

    private static Func<INamedTypeSymbol, bool> IsSubtypeOf(INamedTypeSymbol gameStateInterface)
    {
        return classSymbol =>
        {
            var currentType = classSymbol.BaseType;
            while (currentType != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentType, gameStateInterface))
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        };
    }

    private static void GenerateClassProperties(GeneratorExecutionContext context, INamedTypeSymbol classSymbol)
    {
        // Get all properties of the class
        var properties = GetClassProperties("", classSymbol, $"(({classSymbol.Name})t).");

        var source = $$"""
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

        // Add the generated source to the compilation
        context.AddSource(classSymbol.Name + ".g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static Func<PropertyAccess, string> Selector()
    {
        return AccessorMethodSource;
    }

    private static string AccessorMethodSource(PropertyAccess valueTuple)
    {
        var pathString = valueTuple.GsiPath;
        return $"[\"{pathString}\"]\t=\t(t) => {valueTuple.AccessPath}";
    }

    private static IEnumerable<PropertyAccess> GetClassProperties(string currentPath, INamedTypeSymbol type, string upperAccessPath)
    {
        var namedTypeSymbols = GetBaseTypes(type);
        foreach (var currentType in namedTypeSymbols)
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is not IPropertySymbol property) continue;
                if (property.GetMethod?.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) continue;
                if (property.Name == "Parent") continue;
                if (property.IsIndexer) continue;
                if (property.Name.Contains('<')) continue;
                if (property.Name.Contains('>')) continue;
                if (property.GetAttributes().Any(IgnoredAttribute)) continue;

                var propertyType = property.Type;

                var accessPath = property.IsStatic ? GetStaticAccessPath(currentType, property) : upperAccessPath + property.Name;
                
                // Check if the type is a primitive type
                if (propertyType.IsValueType || propertyType.SpecialType == SpecialType.System_String)
                {
                    yield return new PropertyAccess(currentPath + property.Name, accessPath);
                    continue;
                }
                // Check if the type is a class

                if (propertyType.TypeKind is not (TypeKind.Class or TypeKind.Interface)) continue;

                var namedTypeSymbol = (INamedTypeSymbol)property.Type;
                if (!IsAuroraClass(namedTypeSymbol))
                    continue;
                // prevent infinite recursions, this should look for all previous types actually
                if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol, currentType))
                {
                    continue;
                }

                var upperProperty = currentPath + property.Name + "/";

                var s = property.NullableAnnotation == NullableAnnotation.Annotated ? "?." : ".";
                var lowerAccessPath = property.IsStatic ? GetStaticAccessPath(currentType, property) : upperAccessPath + property.Name + s;
                
                foreach (var classProperty in GetClassProperties(upperProperty, namedTypeSymbol, lowerAccessPath))
                {
                    yield return classProperty;
                }
            }
        }

    }

    private static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol type)
    {
        var currentType = type;
        while (currentType != null)
        {
            yield return currentType;
            currentType = currentType.BaseType;
        }
    }

    private static bool IgnoredAttribute(AttributeData arg)
    {
        return arg.AttributeClass?.Name == "GameStateIgnoreAttribute";
    }

    private static string GetStaticAccessPath(INamedTypeSymbol currentType, IPropertySymbol property)
    {
        return currentType.ContainingNamespace + "." + currentType.Name + "." + property.Name + ".";
    }

    private static bool IsAuroraClass(INamedTypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.ToString().StartsWith("AuroraRgb.");
    }

    private static bool IsPartialClass(INamedTypeSymbol namedTypeSymbol)
    {
        // Loop through all the declaration syntax references
        foreach (var syntaxNode in namedTypeSymbol.DeclaringSyntaxReferences.Select(syntaxReference => syntaxReference.GetSyntax()))
        {
            // Check if the syntax node is a class declaration
            if (syntaxNode is not ClassDeclarationSyntax classDeclaration) continue;
            // Check if the class declaration has the 'partial' modifier
            if (classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return true;
            }
        }

        // If no 'partial' modifier is found, return false
        return false;
    }
}