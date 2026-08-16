using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nubbin.Generator;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor PartialStubError = new(
        id: "NUBBIN001",
        title: "Stub class must be partial",
        messageFormat: "'{0}' is annotated with [Stub] but is not partial",
        category: "Nubbin",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PartialAutoStubError = new(
        id: "NUBBIN002",
        title: "Class containing auto-stubs must be partial",
        messageFormat: "'{0}' contains auto-stubs but is not partial",
        category: "Nubbin",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static bool CheckPartial(
        this SourceProductionContext productionContext,
        INamedTypeSymbol symbol,
        Func<Location> locationGetter,
        DiagnosticDescriptor diagnostic)
    {
        if (!symbol.IsPartial())
        {
            productionContext.ReportDiagnostic(
                Diagnostic.Create(diagnostic, locationGetter(), symbol.Name));
            return false;
        }
        return true;
    }

    private static bool IsPartial(this INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Any(syntax =>
            syntax.GetSyntax() is ClassDeclarationSyntax declaration
            && declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }
}
