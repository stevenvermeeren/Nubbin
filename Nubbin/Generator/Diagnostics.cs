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

    public static readonly DiagnosticDescriptor AutoStubMissingUsingError = new(
        id: "NUBBIN002",
        title: "Class containing auto-stubs must be import Nubbin namespace",
        messageFormat: "'{0}' contains auto-stubs but does not import Nubbin namespace",
        category: "Nubbin",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
