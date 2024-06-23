using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AuroraSourceGenerator;

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
        var properties = GetClassProperties("", classSymbol);

        var source = $$"""
                       // Auto-generated code
                       // {{DateTime.Now}}
                       #nullable enable

                       using System;
                       using System.Collections.Generic;

                       namespace {{classSymbol.ContainingNamespace}}
                       {
                           public partial class {{classSymbol.Name}}
                           {
                               private static readonly Dictionary<string, Func<AuroraRgb.Profiles.IGameState, object?>> _innerProperties = new()
                                {
                       {{string.Join(",\n", properties.Select(Selector(classSymbol)))}}
                                };
                                public override IReadOnlyDictionary<string, Func<AuroraRgb.Profiles.IGameState, object?>> PropertyMap => _innerProperties;
                           }
                       }
                       """;

        // Add the generated source to the compilation
        context.AddSource(classSymbol.Name + ".g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static Func<(string, bool), string> Selector(INamedTypeSymbol classSymbol)
    {
        return p => AccessorMethodSource(classSymbol, p);
    }

    private static string AccessorMethodSource(INamedTypeSymbol classSymbol, (string, bool) valueTuple)
    {
        var p = valueTuple.Item1;
        var containsStatic = valueTuple.Item2;
        var pathString = p.Replace('.', '/');
        if (containsStatic)
        {
            return $"[\"{pathString}\"]\t=\t(t) => ((dynamic)t).{p}";
        }

        return $"[\"{pathString}\"]\t=\t(t) => (({classSymbol.Name})t).{p}";
    }

    private static IEnumerable<(string, bool)> GetClassProperties(string currentPath, INamedTypeSymbol type, bool containsStatic = false)
    {
        var currentType = type;
        while (currentType != null)
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is not IPropertySymbol property) continue;
                if (property.GetMethod?.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) continue;
                if (property.Name == "Parent") continue;
                if (string.IsNullOrWhiteSpace(property.Name)) continue;
                if (property.Name.Contains('[')) continue;
                if (property.Name.Contains(']')) continue;
                if (property.Name.Contains('<')) continue;
                if (property.Name.Contains('>')) continue;

                var propertyContainsStatic = containsStatic || property.IsStatic;

                var propertyType = property.Type;

                // Check if the type is a primitive type
                if (propertyType.IsValueType || propertyType.SpecialType == SpecialType.System_String)
                {
                    yield return (currentPath + property.Name, propertyContainsStatic);
                    continue;
                }
                // Check if the type is a class

                if (propertyType.TypeKind is not (TypeKind.Class or TypeKind.Interface)) continue;

                var namedTypeSymbol = (INamedTypeSymbol)property.Type;
                if (!IsAuroraClass(namedTypeSymbol))
                    continue;
                // prevent infinite recursions
                if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol, currentType))
                {
                    continue;
                }

                var upperProperty = currentPath + property.Name + (property.NullableAnnotation == NullableAnnotation.Annotated ? "?." : ".");
                foreach (var classProperty in GetClassProperties(upperProperty, namedTypeSymbol, propertyContainsStatic))
                {
                    yield return classProperty;
                }
            }

            currentType = currentType.BaseType;
        }

        Console.WriteLine($"{type} property scan complete");
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