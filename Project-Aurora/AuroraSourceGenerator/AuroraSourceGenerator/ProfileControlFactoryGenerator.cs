using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AuroraSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class ProfileControlFactoryGenerator : IIncrementalGenerator
{
    private const string ProfilesNamespace = "AuroraRgb.Profiles";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: ClassPredicate,
                    transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node
                )
                .Collect(),
            (spc, profileClasses) =>
            {
                var mapEntries = profileClasses
                    .Select(cls =>
                    {
                        var profileNamespace = ClassUtils.TryGetParentSyntax(cls, out var parent)
                            ? parent!.Name.ToString()
                            : ProfilesNamespace;
                        var profileName = cls.Identifier.Text;
                        var fullClassName = $"{profileNamespace}.{profileName}";
                        return $"{{ typeof({fullClassName}), app => new {fullClassName}(app) }}";
                    });

                var mapSource = $@"
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using AuroraRgb.Profiles;


namespace AuroraRgb
{{
    public static class ProfileControlFactory
    {{
        public static readonly Dictionary<Type, Func<Application, UserControl>> ApplicationControls = new()
        {{
            {string.Join(",\n            ", mapEntries)}
        }};
    }}
}}
";
                spc.AddSource("ProfileControlFactory.g.cs", mapSource);
            }
        );
    }

    /// <summary>
    /// In ProfilesNamespace namespace and starts with Control_
    /// </summary>
    /// <param name="s"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static bool ClassPredicate(SyntaxNode s, CancellationToken cancellationToken)
    {
        if (s is not ClassDeclarationSyntax classDecl)
            return false;

        // Check if the class is in the Profiles namespace
        if(!ClassUtils.TryGetParentSyntax(classDecl, out var parent))
            return false;
        var parentNamespace = parent!.Name.ToString();
        if (!parentNamespace.StartsWith(ProfilesNamespace))
            return false;

        // check for constructor with Application parameter
        var constructors = classDecl.Members
            .OfType<ConstructorDeclarationSyntax>();
        var hasApplicationConstructor = constructors
            .Any(ctor => ctor.ParameterList.Parameters.Count == 1 && IsApplicationAssignable(ctor.ParameterList.Parameters[0].Type!));
        if (!hasApplicationConstructor)
            return false;

        return true;
    }
    
    private static bool IsApplicationAssignable(TypeSyntax classDecl)
    {
        if (classDecl is not IdentifierNameSyntax identifierName)
            return false;
        
        var classSymbol = identifierName.Identifier.Text;
        
        //true if type name is Application or derived from it
        return classSymbol.EndsWith("Application");
    }
}