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
    private const string NewtonsoftGameStateInterface = "AuroraRgb.Profiles.NewtonsoftGameState";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all class declarations from syntax
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsAuroraGameStateSubclass(s),
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static m => m is not null);

        // Combine the compilation and the class declarations
        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<ClassDeclarationSyntax> Classifications)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the source
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Compilation, source.Classifications, spc));
    }

    private static bool IsAuroraGameStateSubclass(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        if (TryGetParentSyntax(classDeclaration, out var parent))
        {
            var parentNamespace = parent!.Name.ToString();
            var rootNamespace = parentNamespace.Split('.').FirstOrDefault();

            if (rootNamespace != "AuroraRgb")
            {
                return false;
            }
        }

        return IsSubtypeOf(classDeclaration, "GameState");
    }

    private static bool TryGetParentSyntax(SyntaxNode? syntaxNode, out NamespaceDeclarationSyntax? result)
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
                case NamespaceDeclarationSyntax r:
                    result = r;
                    return true;
                default:
                    continue;
            }
        }
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        HashSet<string> ignore = [GameStateInterface];
        HashSet<string> ignoredInterfaces = [NewtonsoftGameStateInterface];

        var lookups = classes.Select(classDeclaration =>
            {
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                return semanticModel.GetDeclaredSymbol(classDeclaration);
            }).Where(classSymbol => classSymbol != null)
            .Where(classSymbol => !ignore.Contains(classSymbol!.ToDisplayString()))
            // Filter out classes that implement ignored interfaces
            .Where(classSymbol => !classSymbol!.AllInterfaces.Any(i => ignoredInterfaces.Contains(i.ToDisplayString())))
            // Filter out classes that are not AuroraRgb classes
            // to dictionary where classSymbol is the key
            .ToDictionary(classSymbol => classSymbol!.ToDisplayString(), classSymbol => GenerateClassProperties(context, classSymbol!));


        if (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        var source = NodePropertyLookupsGenerator.GetSource(lookups);
        context.AddSource("NodePropertyLookups.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static bool IsSubtypeOf(ClassDeclarationSyntax classSymbol, string @interface)
    {
        return classSymbol.BaseList?.Types
            .Select(a => a.Type.ToString())
            .Any(name => name == @interface) ?? false;
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

        if (context.CancellationToken.IsCancellationRequested)
        {
            return [];
        }

        return properties
            .OrderBy(p => p.GsiPath)
            .ToList();
    }
}