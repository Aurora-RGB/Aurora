using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AuroraSourceGenerator;

internal class PropertySetter(string accessPath, ITypeSymbol valueType, INamedTypeSymbol baseType, bool propertyIsOverride)
{
    public string AccessPath { get; } = accessPath;
    public ITypeSymbol ValueType { get; } = valueType;
    public INamedTypeSymbol BaseType { get; } = baseType;
    public bool IsOverride { get; } = propertyIsOverride;
}

[Generator(LanguageNames.CSharp)]
public class OverrideSetterSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        const string layerHandlerProperties = "AuroraRgb.Settings.Layers.LayerHandlerProperties";

        // Get all class declarations and their semantic models
        var classDeclarations =
            context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: (ctx, _) =>
                    {
                        var syntax = (ClassDeclarationSyntax)ctx.Node;
                        var symbol = ctx.SemanticModel.GetDeclaredSymbol(syntax) as INamedTypeSymbol;
                        return (Syntax: syntax, Symbol: symbol);
                    });

        // Get the handlerPropertiesInterface
        var handlerInterface = context.CompilationProvider.Select((c, _) => c.GetTypeByMetadataName(layerHandlerProperties));

        // Combine the class declarations with the interface
        var relevantClasses = classDeclarations
            .Where(tuple => tuple.Symbol != null)
            .Combine(handlerInterface.Select((i, _) => i!))
            .Where(tuple => IsSubtypeOf(tuple.Right)(tuple.Left.Symbol!))
            .Select((tuple, _) => (Class: tuple.Left.Symbol!, Interface: tuple.Right));

        // Generate the individual class logic files
        context.RegisterSourceOutput(relevantClasses,
            (spc, tuple) =>
            {
                try
                {
                    GenerateLogic(spc, tuple.Class);
                    GenerateLogicOverridePartial(spc, tuple.Class);
                }
                catch (Exception e)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "ASG001",
                            "Generator Error",
                            "Error generating source: {0}",
                            "SourceGenerator",
                            DiagnosticSeverity.Error,
                            true),
                        Location.None,
                        e.ToString()));
                }
            });

        // Collect all generated classes for the GeneratedLogics file
        var allGeneratedClasses =
            relevantClasses
                .Collect()
                .Select((items, _) =>
                {
                    var result = new List<INamedTypeSymbol>();
                    if (items.Any())
                    {
                        result.Add(items.First().Interface);
                        result.AddRange(items.Select(i => i.Class));
                    }

                    return result.ToImmutableArray();
                });

        // Generate the GeneratedLogics file
        context.RegisterSourceOutput(allGeneratedClasses,
            (spc, classes) =>
            {
                var source = $$"""
                               // Auto-generated code
                               // {{DateTime.Now}}
                               #nullable enable

                               using System;
                               using System.Collections.Generic;

                               namespace AuroraRgb.Settings.Layers
                               {
                                   public static class GeneratedLogics
                                   {
                                       private static readonly Dictionary<string, Func<LayerHandlerPropertiesLogic>> _innerLogics = new()
                                       {
                               {{string.Join(",\n", classes.Where(c => !c.IsGenericType).Select(GetLogicInstanceSource))}}
                                       };
                                       public static IReadOnlyDictionary<string, Func<LayerHandlerPropertiesLogic>> LogicMap => _innerLogics;
                                   }
                               }
                               """;

                spc.AddSource("GeneratedLogics.g.cs", SourceText.From(source, Encoding.UTF8));
            });
    }

    // Rest of the existing helper methods remain the same
    private static string GetLogicInstanceSource(INamedTypeSymbol arg) => $"[\"{arg.Name}\"]\t=\t() => new {arg}Logic()";

    private static Func<INamedTypeSymbol, bool> IsSubtypeOf(INamedTypeSymbol upperClass)
    {
        return classSymbol =>
        {
            var currentType = classSymbol.BaseType;
            while (IsAuroraClass(currentType) && currentType != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentType.ConstructedFrom, upperClass))
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        };
    }

    private static void GenerateLogic(SourceProductionContext context, INamedTypeSymbol propertiesClassSymbol)
    {
        string[] ignore = ["LayerHandlerProperties"];

        if (ignore.Contains(propertiesClassSymbol.Name))
        {
            return;
        }

        var properties = GetClassProperties(propertiesClassSymbol)
            .Where(p => p.AccessPath != "Logic")
            .ToList();
        var logicClassName = AppendClassName(propertiesClassSymbol.Name, "Logic");

        var genericParams = "";
        if (propertiesClassSymbol.IsGenericType)
        {
            genericParams = "<" + string.Join(",", propertiesClassSymbol.TypeArguments.Select(t => t.ToString())) + ">";
        }

        var source = $$"""
                       // Auto-generated code
                       // {{DateTime.Now}}
                       // GenerateClassProperties
                       #nullable enable

                       using System;
                       using System.Collections.Generic;

                       namespace {{propertiesClassSymbol.ContainingNamespace}}
                       {
                           public partial class {{logicClassName}}{{genericParams}} : {{AppendClassName(propertiesClassSymbol.BaseType.ToString(), "Logic")}}
                           {
                       {{string.Join("\n", properties.Where(p => !p.IsOverride).Select((Func<PropertySetter, string>)PropertyDefinitionSource))}}
                       
                               private static readonly Dictionary<string, Action<AuroraRgb.Settings.Layers.LayerHandlerPropertiesLogic, object?>> InnerSetters = new()
                               {
                       {{string.Join(",\n", properties.Select((Func<PropertySetter, string>)SetMethodSource))}}
                               };
                               public override IReadOnlyDictionary<string, Action<AuroraRgb.Settings.Layers.LayerHandlerPropertiesLogic, object?>> SetterMap => InnerSetters;
                           }
                       }
                       """;

        var genericFileSuffix = string.Join(".", propertiesClassSymbol.TypeArguments.Select(t => t.ToString()));
        context.AddSource(logicClassName + genericFileSuffix + ".g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static void GenerateLogicOverridePartial(SourceProductionContext context, INamedTypeSymbol classSymbol)
    {
        var logicClassName = AppendClassName(classSymbol.ToString(), "Logic");

        var genericParams = "";
        if (classSymbol.IsGenericType)
        {
            genericParams = "<" + string.Join(",", classSymbol.TypeArguments.Select(t => t.ToString())) + ">";
        }

        var genericConstraints = "";
        if (classSymbol.IsGenericType)
        {
            var constraints = classSymbol.TypeParameters
                .SelectMany(t => t.ConstraintTypes.Select(c => t.Name + " : " + c));
            genericConstraints = "where " + string.Join(",", constraints);
        }

        var source = $$"""
                       // Auto-generated code
                       // GenerateLogicOverridePartial
                       // {{DateTime.Now}}
                       #nullable enable

                       using System;
                       using System.Collections.Generic;
                       using Newtonsoft.Json;
                       using AuroraRgb.Profiles;

                       namespace {{classSymbol.ContainingNamespace}}
                       {
                           public partial class {{classSymbol.Name}}{{genericParams}}
                               {{genericConstraints}}
                           {
                               [GameStateIgnore, JsonIgnore] 
                               public new {{logicClassName}}? Logic
                                {
                                    get => ({{logicClassName}}?)base.Logic;
                                    set
                                    {
                                        base.Logic = value;
                                    }
                                }
                           }
                       }
                       """;

        var genericFileSuffix = string.Join(".", classSymbol.TypeArguments.Select(t => t.ToString()));
        context.AddSource(classSymbol.Name + genericFileSuffix + ".g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string AppendClassName(string className, string append)
    {
        if (className.Contains('<'))
        {
            var i = className.IndexOf('<');
            return className.Substring(0, i) + append + className.Substring(i);
        }

        return className + append;
    }

    private static string PropertyDefinitionSource(PropertySetter valueTuple)
    {
        var valueType = valueTuple.ValueType;
        var accessPath = valueTuple.AccessPath;

        var fieldName = $"_{accessPath}";
        var propertyType = valueType.ToString().EndsWith("?") ? valueType.ToString() : valueType + "?";

        var firstCharIndex = accessPath.LastIndexOf('_') + 1;

        var lowerPropertyName = char.ToLower(accessPath[firstCharIndex]) + accessPath.Substring(firstCharIndex + 1);
        var upperPropertyName = char.ToUpper(accessPath[firstCharIndex]) + accessPath.Substring(firstCharIndex + 1);

        string[] properties =
        [
            lowerPropertyName,
            upperPropertyName,
            $"_{lowerPropertyName}",
            $"_{upperPropertyName}",
            accessPath
        ];

        var propertySources = properties.Distinct()
            .Except([fieldName])
            .Select(p => $$"""
                           public {{propertyType}} {{p}}
                           {
                               get => {{fieldName}};
                               set
                               {
                                   {{fieldName}} = value;
                               }
                           }

                           """);

        return $"""
                // field
                public {propertyType} {fieldName};

                {string.Join("", propertySources)}
                """;
    }

    private static string SetMethodSource(PropertySetter valueTuple)
    {
        var propertyPath = $"(({AppendClassName(valueTuple.BaseType.ToString(), "Logic")})t).{valueTuple.AccessPath}";
        if (valueTuple.ValueType.TypeKind == TypeKind.Enum)
        {
            return $$"""
                                   ["{{valueTuple.AccessPath}}"]   =   (t, v) => {
                                      var value = (v == null) ? ({{valueTuple.ValueType}})0 : ({{valueTuple.ValueType}})v;
                                      if({{propertyPath}} == value) return;
                                      {{propertyPath}} = value;
                                   }
                     """;
        }

        if (valueTuple.ValueType.IsRecord || valueTuple.ValueType.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return $$"""
                                   ["{{valueTuple.AccessPath}}"]	=	(t, v) => {
                                      if({{propertyPath}} == ({{valueTuple.ValueType}})v) return;
                                      {{propertyPath}} = ({{valueTuple.ValueType}})v;
                                   }
                     """;
        }

        if (valueTuple.ValueType.IsValueType)
        {
            return $$"""
                                   ["{{valueTuple.AccessPath}}"]	=	(t, v) => {
                                      if({{propertyPath}} == ({{valueTuple.ValueType}}?)v) return;
                                      {{propertyPath}} = (v as {{valueTuple.ValueType}}?)!;
                                   }
                     """;
        }

        return $$"""
                               ["{{valueTuple.AccessPath}}"]	=	(t, v) => {
                                  if({{propertyPath}} == v as {{RemoveNullable(valueTuple.ValueType.ToString())}}) return;
                                  {{propertyPath}} = v as {{RemoveNullable(valueTuple.ValueType.ToString())}};
                               }
                 """;
    }

    private static string RemoveNullable(string typeName)
    {
        if (typeName.EndsWith("?"))
        {
            return typeName.Substring(0, typeName.Length - 1);
        }

        return typeName;
    }

    private static IEnumerable<PropertySetter> GetClassProperties(INamedTypeSymbol classSymbol)
    {
        return ClassUtils.GetBaseTypes(classSymbol)
            .SelectMany(c => c.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(property => property.GetMethod?.DeclaredAccessibility.HasFlag(Accessibility.Public) ??
                               (property.SetMethod?.DeclaredAccessibility.HasFlag(Accessibility.Internal) ?? false))
            .Where(property => !property.IsStatic)
            .Where(p => IsLogicOverridable(p) || !p.Name.StartsWith("_"))
            .Select(symbol => GetMemberSetter(symbol, classSymbol));
    }

    private static bool IsLogicOverridable(IPropertySymbol propertySymbol)
    {
        return propertySymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "LogicOverridableAttribute");
    }

    private static PropertySetter GetMemberSetter(IPropertySymbol property, INamedTypeSymbol baseType)
    {
        return new PropertySetter(property.Name, property.Type, baseType, !SymbolEqualityComparer.IncludeNullability.Equals(property.ContainingType, baseType));
    }

    private static bool IsAuroraClass(INamedTypeSymbol? namedTypeSymbol)
    {
        return namedTypeSymbol != null && namedTypeSymbol.ToString().StartsWith("AuroraRgb.");
    }
}