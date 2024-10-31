using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

[Generator]
public class NodePropertySourceGenerator : IIncrementalGenerator
{
    private const string GameStateInterface = "AuroraRgb.Profiles.GameState";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all class declarations from syntax
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // Combine the compilation and the class declarations
        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<ClassDeclarationSyntax> Classifications)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the source
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Compilation, source.Classifications, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax classDeclaration
           && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Get the semantic model
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol == null) return null;

        // Check if it's an Aurora class and derives from GameState
        if (!IsAuroraClass(classSymbol)) return null;

        var gameStateInterface = semanticModel.Compilation.GetTypeByMetadataName(GameStateInterface);
        if (gameStateInterface == null) return null;

        if (!IsSubtypeOf(classSymbol, gameStateInterface)) return null;

        return classDeclaration;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        HashSet<string> ignore = [GameStateInterface];

        foreach (var classDeclaration in classes)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null) continue;
            if (ignore.Contains(classSymbol.ToDisplayString())) continue;

            GenerateClassProperties(context, classSymbol);
        }
    }

    private static bool IsSubtypeOf(INamedTypeSymbol classSymbol, INamedTypeSymbol gameStateInterface)
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
    }

    private static void GenerateClassProperties(SourceProductionContext context, INamedTypeSymbol classSymbol)
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

        context.AddSource(classSymbol.Name + ".g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static Func<PropertyAccess, string> Selector() => AccessorMethodSource;

    private static string AccessorMethodSource(PropertyAccess valueTuple)
    {
        var pathString = valueTuple.GsiPath;
        return $"[\"{pathString}\"]\t=\t(t) => {valueTuple.AccessPath}";
    }

    private static IEnumerable<PropertyAccess> GetClassProperties(string currentPath, INamedTypeSymbol type, string upperAccessPath)
    {
        var namedTypeSymbols = ClassUtils.GetBaseTypes(type);
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

                if (propertyType.IsValueType || propertyType.SpecialType == SpecialType.System_String)
                {
                    yield return new PropertyAccess(currentPath + property.Name, accessPath);
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

    private static bool IgnoredAttribute(AttributeData arg)
        => arg.AttributeClass?.Name == "GameStateIgnoreAttribute";

    private static string GetStaticAccessPath(INamedTypeSymbol currentType, IPropertySymbol property)
        => currentType.ContainingNamespace + "." + currentType.Name + "." + property.Name + ".";

    private static bool IsAuroraClass(INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ToString().StartsWith("AuroraRgb.");
}