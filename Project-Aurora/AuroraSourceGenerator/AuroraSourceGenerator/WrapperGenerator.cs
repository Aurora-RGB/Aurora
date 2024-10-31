using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AuroraSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class WrapperGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "AuroraSourceGenerator";
    private const string DelegateToAttributeClassname = "DelegateToAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "DelegateToAttribute.g.cs",
            SourceText.From($$"""
                             using System;

                             namespace {{AttributeNamespace}}
                             {
                                 [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
                                 public class {{DelegateToAttributeClassname}}(string field) : System.Attribute
                                 {
                                     public string Field { get; } = field;
                                 }
                             }
                             """, Encoding.UTF8)
        ));

        // Create a pipeline for finding decorated classes
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetClassToGenerate(ctx))
            .Where(static m => m is not null);

        // Register the source output
        context.RegisterSourceOutput(classDeclarations, 
            static (spc, classInfo) => Execute(spc, classInfo!));
    }

    private static ClassToGenerate? GetClassToGenerate(GeneratorSyntaxContext context)
    {
        var wrappedClass = (ClassDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        if (model.GetDeclaredSymbol(wrappedClass) is not INamedTypeSymbol wrapperClass)
        {
            return null;
        }

        var delegateAttribute = wrapperClass.GetAttributes()
            .FirstOrDefault(attr => DelegateToAttributeClassname == attr.AttributeClass?.Name);
        
        if (delegateAttribute == null)
        {
            return null;
        }

        var delegateFieldName = delegateAttribute.ConstructorArguments[0].Value?.ToString();
        if (delegateFieldName == null)
        {
            return null;
        }

        var delegateField = wrapperClass.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(field => field.Name == delegateFieldName);

        if (delegateField == null)
        {
            return null;
        }

        var definedMethodNames = ClassUtils.GetBaseTypes(wrapperClass)
            .SelectMany(c => c.GetMembers())
            .OfType<IMethodSymbol>()
            .Where(m => !m.IsStatic && m.MethodKind == MethodKind.Ordinary)
            .Where(IsPublic)
            .Select(s => s.Name)
            .ToImmutableHashSet();

        var definedPropertyNames = ClassUtils.GetBaseTypes(wrapperClass)
            .SelectMany(c => c.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic)
            .Where(IsPublic)
            .Select(s => s.Name)
            .ToImmutableHashSet();

        var delegateClass = delegateField.Type;

        // Collect methods and properties
        var methods = ClassUtils.GetBaseTypes(delegateClass)
            .Union<ITypeSymbol>(ClassUtils.GetAllInterfaces(wrapperClass), SymbolEqualityComparer.IncludeNullability)
            .SelectMany(c => c.GetMembers())
            .OfType<IMethodSymbol>()
            .Distinct<IMethodSymbol>(SymbolEqualityComparer.IncludeNullability)
            .Where(m => !m.IsStatic && m.MethodKind == MethodKind.Ordinary)
            .Where(IsPublic)
            .Where(m => !definedMethodNames.Contains(m.Name))
            .Select(GetMethodToGenerate)
            .ToList();

        var properties = ClassUtils.GetBaseTypes(delegateClass)
            .SelectMany(c => c.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic)
            .Where(IsPublic)
            .Where(m => !definedPropertyNames.Contains(m.Name))
            .Select(GetPropertyToGenerate)
            .ToList();

        return new ClassToGenerate(
            wrapperClass.Name,
            wrapperClass.ContainingNamespace.ToDisplayString(),
            delegateFieldName,
            methods,
            properties);
    }

    private static void Execute(SourceProductionContext context, ClassToGenerate classInfo)
    {
        var source = GenerateWrapperClass(classInfo);
        context.AddSource($"{classInfo.ClassName}.Wrapper.g.cs", 
            SourceText.From(source, Encoding.UTF8));
    }

    private static string GenerateWrapperClass(ClassToGenerate classInfo)
    {
        var sb = new StringBuilder();

        sb.AppendLine($$"""
                        namespace {{classInfo.Namespace}}
                        {
                            public partial class {{classInfo.ClassName}}
                            {
                        """
        );

        // Generate delegating methods and properties
        foreach (var property in classInfo.Properties)
        {
            sb.AppendLine($$"""
                                    public {{property.Type}} {{property.Name}}
                                    {
                            """
            );
            if (property.HasGetter)
            {
                sb.AppendLine($"            get => {classInfo.FieldName}.{property.Name};");
            }
            if (property.HasSetter)
            {
                sb.AppendLine($"            set => {classInfo.FieldName}.{property.Name} = value;");
            }
            sb.AppendLine("""        }"""
            );
        }

        foreach (var method in classInfo.Methods)
        {
            if (method.ReturnType != "void")
            {
                sb.AppendLine($$"""
                                    public {{method.ReturnType}} {{method.Name}}({{method.Parameters}})
                                        {
                                            return {{classInfo.FieldName}}.{{method.Name}}({{method.ParameterNames}});
                                        }
                                """
                );
            }
            else
            {
                sb.AppendLine($$"""
                                    public {{method.ReturnType}} {{method.Name}}({{method.Parameters}})
                                        {
                                            {{classInfo.FieldName}}.{{method.Name}}({{method.ParameterNames}});
                                        }
                                """
                );
            }
        }

        sb.AppendLine("""
                          }
                      }
                      """
        );

        return sb.ToString();
    }

    private static bool IsPublic(ISymbol m)
    {
        return m.DeclaredAccessibility.HasFlag(Accessibility.Public);
    }

    private static PropertyToGenerate GetPropertyToGenerate(IPropertySymbol p)
    {
        return new PropertyToGenerate(p.Name, p.Type.ToDisplayString(), p.GetMethod != null, p.SetMethod != null);
    }

    private static MethodToGenerate GetMethodToGenerate(IMethodSymbol m)
    {
        return new MethodToGenerate(
            m.Name,
            m.ReturnType.ToDisplayString(),
            string.Join(", ", m.Parameters.Select(ParameterNameWithModifiers)),
            string.Join(", ", m.Parameters.Select(ReferenceName))
        );

        static string ParameterNameWithModifiers(IParameterSymbol p)
        {
            var typeAndName = $"{p.Type.ToDisplayString()} {p.Name}";

            var modifiers = p.RefKind switch
            {
                RefKind.None => string.Empty,
                RefKind.In => "in ",
                RefKind.Out => "out ",
                RefKind.Ref => "ref ",
                RefKind.RefReadOnlyParameter => "ref readonly ",
            };
            var defaultPart = DefaultPart(p);
            return modifiers + typeAndName + defaultPart;
        }

        static string ReferenceName(IParameterSymbol p)
        {
            return p.RefKind switch
            {
                RefKind.RefReadOnlyParameter => "in " + p.Name,
                _ => p.Name,
            };
        }

        static string DefaultPart(IParameterSymbol p)
        {
            if (!p.HasExplicitDefaultValue) return string.Empty;
            if (p.Type.TypeKind == TypeKind.Enum)
            {
                if (p.ExplicitDefaultValue == null)
                    return " = default";
                var enumName = p.Type.ToDisplayString();
                return $" = ({enumName})" + p.ExplicitDefaultValue;
            }
            if (p.Type.IsValueType)
            {
                return " = " + (p.ExplicitDefaultValue ?? "default").ToString().ToLowerInvariant();
            }

            return " = " + (p.ExplicitDefaultValue ?? "default");
        }
    }

    private sealed record ClassToGenerate(
        string ClassName,
        string Namespace,
        string FieldName,
        List<MethodToGenerate> Methods,
        List<PropertyToGenerate> Properties);

    private sealed record MethodToGenerate(string Name, string ReturnType, string Parameters, string ParameterNames);

    private sealed record PropertyToGenerate(string Name, string Type, bool HasGetter, bool HasSetter);
}