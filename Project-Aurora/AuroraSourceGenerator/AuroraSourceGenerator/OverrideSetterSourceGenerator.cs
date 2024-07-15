using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AuroraSourceGenerator;

internal class PropertySetter(string gsiPath, string accessPath, ITypeSymbol valueType)
{
    public string GsiPath { get; } = gsiPath;
    public string AccessPath { get; } = accessPath;
    public ITypeSymbol ValueType { get; } = valueType;
}

/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator]
public class OverrideSetterSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this example
    }

    public void Execute(GeneratorExecutionContext context)
    {
        const string layerHandlerProperties = "AuroraRgb.Settings.Layers.LayerHandlerProperties`1";
        HashSet<string> ignore = [layerHandlerProperties];

        var compilation = context.Compilation;

        // Retrieve the interface symbol
        var handlerPropertiesInterface = compilation.GetTypeByMetadataName(layerHandlerProperties);

        if (handlerPropertiesInterface == null)
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
            .Where(IsSubtypeOf(handlerPropertiesInterface))
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
            while (IsAuroraClass(currentType) && currentType != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentType.ConstructedFrom, gameStateInterface))
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

                       namespace {{classSymbol.ContainingNamespace}}
                       {
                           public partial class {{classSymbol.Name}}
                           {
                               private static readonly Dictionary<string, Action<AuroraRgb.Settings.Layers.IValueOverridable, object?>> _innerSetters = new()
                                {
                       {{string.Join(",\n", properties.Select((Func<PropertySetter, string>)SetMethodSource))}}
                                };
                                public override IReadOnlyDictionary<string, Action<AuroraRgb.Settings.Layers.IValueOverridable, object?>> SetterMap => _innerSetters;
                           }
                       }
                       """;

        // Add the generated source to the compilation
        context.AddSource(classSymbol.Name + ".g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string SetMethodSource(PropertySetter valueTuple)
    {
        if (valueTuple.ValueType.TypeKind == TypeKind.Enum)
        {
            return $$"""
                                   ["{{valueTuple.GsiPath}}"]   =   (t, v) => {
                                      var value = (v == null) ? ({{valueTuple.ValueType}})0 : ({{valueTuple.ValueType}})v;
                                      if({{valueTuple.AccessPath}} == value) return;
                                      {{valueTuple.AccessPath}} = value;
                                   }
                     """;
        }

        if (valueTuple.ValueType.IsRecord)
        {
            return $$"""
                                  ["{{valueTuple.GsiPath}}"]	=	(t, v) => {
                                     if({{valueTuple.AccessPath}} == ({{valueTuple.ValueType}})v) return;
                                     {{valueTuple.AccessPath}} = ({{valueTuple.ValueType}})v;
                                  }
                    """;
        }

        return $$"""
                               ["{{valueTuple.GsiPath}}"]	=	(t, v) => {
                                  if({{valueTuple.AccessPath}} == v as {{valueTuple.ValueType}}) return;
                                  {{valueTuple.AccessPath}} = v as {{valueTuple.ValueType}};
                               }
                 """;
    }

    private static IEnumerable<PropertySetter> GetClassProperties(string currentPath, INamedTypeSymbol type, string upperAccessPath)
    {
        char[] propertyIgnoreChars =
        [
            '[',
            ']',
            '<',
            '>',
        ];

        var currentType = type;
        while (currentType != null)
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is not IPropertySymbol property) continue;
                if (property.GetMethod?.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) continue;
                if (property.Name == "Parent") continue;
                if (string.IsNullOrWhiteSpace(property.Name)) continue;
                if (Array.Exists(propertyIgnoreChars, c => property.Name.Contains(c))) continue;
                if (property.GetAttributes().Any(IgnoredAttribute)) continue;

                var accessPath = property.IsStatic ? GetStaticAccessPath(currentType, property) : upperAccessPath + property.Name;

                // Check if the type is a primitive type
                if (property is { IsStatic: false, IsReadOnly: false, SetMethod: not null })
                {
                    yield return new PropertySetter(currentPath + property.Name, accessPath, property.Type);
                }
            }

            currentType = currentType.BaseType;
        }

        Console.WriteLine($"{type} property scan complete");
    }

    private static bool IgnoredAttribute(AttributeData arg)
    {
        return arg.AttributeClass?.Name == "GameStateIgnoreAttribute";
    }

    private static string GetStaticAccessPath(INamedTypeSymbol currentType, IPropertySymbol property)
    {
        return currentType.ContainingNamespace + "." + currentType.Name + "." + property.Name + ".";
    }

    private static bool IsAuroraClass(INamedTypeSymbol? namedTypeSymbol)
    {
        return namedTypeSymbol != null && namedTypeSymbol.ToString().StartsWith("AuroraRgb.");
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