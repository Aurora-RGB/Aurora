using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator;

public static class AuroraSourceLinter
{
    public static void LintGenericSetterError(SourceProductionContext spc, Exception e)
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

    public static void LintGenericPropertyError(SourceProductionContext context, INamedTypeSymbol classSymbol, Exception ex)
    {
        var classTypeLocation = classSymbol.Locations
            .FirstOrDefault(l => l.IsInSource) ?? Location.None;
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "ASG002",
                    "Error in NodePropertySourceGenerator",
                    $"Error processing class {classSymbol.Name}: {ex.Message}",
                    "SourceGenerator",
                    DiagnosticSeverity.Error,
                    true
                ),
                classTypeLocation
            )
        );
    }

    public static void LintNotPartial(SourceProductionContext context, INamedTypeSymbol classSymbol)
    {
        var classTypeLocation = classSymbol.Locations
            .FirstOrDefault(l => l.IsInSource) ?? Location.None;
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "ASG003",
                    "NodePropertySourceGenerator",
                    $"Class is not using NewtonsoftGameState and not marked as partial. " +
                    $"Aurora will use reflection & string manipulation to access properties.",
                    "SourceGenerator",
                    DiagnosticSeverity.Warning,
                    true
                ),
                classTypeLocation
            )
        );
    }
}