using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AuroraSourceGenerator.NodeProperties.GeneratedClasses;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AuroraSourceGenerator.NodeProperties;

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

        var lookups = classes.Select(classDeclaration =>
            {
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                return semanticModel.GetDeclaredSymbol(classDeclaration);
            }).Where(classSymbol => classSymbol != null)
            .Where(classSymbol => !ignore.Contains(classSymbol!.ToDisplayString()))
            // to dictionary where classSymbol is the key
            .ToDictionary(classSymbol => classSymbol!.ToDisplayString(), classSymbol => GenerateClassProperties(context, classSymbol!));
        // flatten the list of PropertyLookupInfo


        if (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        var source = NodePropertyLookupsGenerator.GetSource(lookups);
        context.AddSource("NodePropertyLookups.g.cs", SourceText.From(source, Encoding.UTF8));
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

    private static List<PropertyLookupInfo> GenerateClassProperties(SourceProductionContext context, INamedTypeSymbol classSymbol)
    {
        // Get all properties of the class
        var properties = PropertyAnalyzer.GetClassProperties("", classSymbol, $"(({classSymbol.Name})t).")
            .ToImmutableList();

        if (context.CancellationToken.IsCancellationRequested)
        {
            return [];
        }

        var source = PartialGameStateGenerator.GetSource(classSymbol, properties);
        context.AddSource(classSymbol.Name + ".g.cs", SourceText.From(source, Encoding.UTF8));

        var propertyLookupInfos = properties
            .Select(p =>
            {
                var paths = p.GsiPath.Split('/');
                var name = paths.Last();
                return new PropertyLookupInfo(name, p.GsiPath, p.PropertyType);
            })
            .ToList();
        var folderLookupInfos = propertyLookupInfos
            .SelectMany(p =>
            {
                var paths = p.GsiPath.Split('/');
                // get all path depths until the last one

                // IntRange(0, paths.Length - 1) but in C#:
                return Enumerable.Range(1, paths.Length - 1)
                    .Select(i =>
                    {
                        var folderName = paths[i - 1];
                        var folderPath = string.Join("/", paths.Take(i));
                        return new PropertyLookupInfo(folderName, folderPath);
                    });
            })
            .GroupBy(p => p.GsiPath)
            .Select(g => g.First())
            .Where(p => !string.IsNullOrWhiteSpace(p.GsiPath))
            .ToList();

        if (context.CancellationToken.IsCancellationRequested)
        {
            return [];
        }

        return folderLookupInfos.Union(propertyLookupInfos)
            .OrderBy(p => p.GsiPath)
            .ToList();
    }

    private static bool IsAuroraClass(INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ToString().StartsWith("AuroraRgb.");
}