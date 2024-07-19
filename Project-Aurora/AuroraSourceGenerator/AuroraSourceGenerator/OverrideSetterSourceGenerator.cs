using System;
using System.Collections.Generic;
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

[Generator]
public class OverrideSetterSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
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
            .Select(declaration => compilation.GetSemanticModel(declaration.SyntaxTree).GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Where(IsSubtypeOf(handlerPropertiesInterface))
            //.Where(NotGenericClass)
            .Where(classSymbol => !ignore.Contains(classSymbol.ToDisplayString()));

        List<INamedTypeSymbol> generatedClasses = [];
        foreach (var classSymbol in classes)
        {
            try
            {
                GenerateLogic(context, classSymbol);
                GenerateLogicOverridePartial(context, classSymbol);
                generatedClasses.Add(classSymbol);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("uh oh");
                Console.Error.WriteLine(e);
            }
        }
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
                       {{string.Join(",\n", generatedClasses.Select(s => s).Where(c => !c.IsGenericType).Select(GetLogicInstanceSource))}}
                                };
                                public static IReadOnlyDictionary<string, Func<LayerHandlerPropertiesLogic>> LogicMap => _innerLogics;
                           }
                       }
                       """;

        // Add the generated source to the compilation
        context.AddSource("GeneratedLogics.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string GetLogicInstanceSource(INamedTypeSymbol arg)
    {
        return $"[\"{arg.Name}\"]\t=\t() => new {arg}Logic()";
    }

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

    private static void GenerateLogic(GeneratorExecutionContext context, INamedTypeSymbol propertiesClassSymbol)
    {
        string[] ignore = ["LayerHandlerProperties"];

        if (ignore.Contains(propertiesClassSymbol.Name))
        {
            return;
        }

        // Get all properties of the class
        var properties = GetClassProperties(propertiesClassSymbol)
            .Where(p => p.AccessPath != "Logic")
            .ToList();
        var logicClassName = AppendClassName(propertiesClassSymbol.Name, "Logic");
        Console.WriteLine(logicClassName);

        var genericParams = "";
        if (propertiesClassSymbol.IsGenericType)
        {
            genericParams = "<" + string.Join(",",  propertiesClassSymbol.TypeArguments.Select(t => t.ToString())) + ">";
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
        // Add the generated source to the compilation
        context.AddSource(logicClassName + genericFileSuffix + ".g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private void GenerateLogicOverridePartial(GeneratorExecutionContext context, INamedTypeSymbol classSymbol)
    {
        // Get all properties of the class
        var logicClassName = AppendClassName(classSymbol.ToString(), "Logic");

        var genericParams = "";
        if (classSymbol.IsGenericType)
        {
            genericParams = "<" + string.Join(",",  classSymbol.TypeArguments.Select(t => t.ToString())) + ">";
        }

        var genericConstraints = "";
        if (classSymbol.IsGenericType)
        {
            var constraints = classSymbol.TypeParameters
                .SelectMany(t => t.ConstraintTypes.Select(c =>  t.Name + " : " + c));
            genericConstraints = "where " + string.Join(",",  constraints);
        }

        var source = $$"""
                       // Auto-generated code
                       // GenerateLogicOverridePartial
                       // {{DateTime.Now}}
                       #nullable enable

                       using System;
                       using System.Collections.Generic;

                       namespace {{classSymbol.ContainingNamespace}}
                       {
                           public partial class {{classSymbol.Name}}{{genericParams}} : AuroraRgb.Settings.Layers.LogicHolder<{{logicClassName}}>
                               {{genericConstraints}}
                           {
                                public {{logicClassName}}? Logic { get; set; }
                           }
                       }
                       """;

        var genericFileSuffix = string.Join(".", classSymbol.TypeArguments.Select(t => t.ToString()));
        // Add the generated source to the compilation
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

        string[] properties = [
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
        return GetBaseTypes(classSymbol)
            .SelectMany(c => c.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(property => property.GetMethod?.DeclaredAccessibility.HasFlag(Accessibility.Public) ?? (property.SetMethod?.DeclaredAccessibility.HasFlag(Accessibility.Internal) ?? false))
            .Where(property => !property.IsStatic)
            .Where(p => IsLogicOverridable(p) || !p.Name.StartsWith("_"))
            .Select(symbol => GetMemberSetter(symbol, classSymbol));
    }

    private static bool IsLogicOverridable(IPropertySymbol propertySymbol)
    {
        return propertySymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "LogicOverridableAttribute");
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

    private static PropertySetter GetMemberSetter(IPropertySymbol property, INamedTypeSymbol baseType)
    {
        return new PropertySetter(property.Name, property.Type, baseType,  !SymbolEqualityComparer.IncludeNullability.Equals(property.ContainingType, baseType));
    }

    private static bool IsAuroraClass(INamedTypeSymbol? namedTypeSymbol)
    {
        return namedTypeSymbol != null && namedTypeSymbol.ToString().StartsWith("AuroraRgb.");
    }
}