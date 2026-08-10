using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nubbin.Generator;

/// <summary>
/// Generates dummy implementations for classes marked with <see cref="StubAttribute"/>.
/// </summary>
[Generator]
public sealed class StubGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Registers the incremental pipeline that discovers <see cref="StubAttribute"/> targets.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Nubbin.StubAttribute",
            static (node, _) => node is ClassDeclarationSyntax,
            static (syntaxContext, _) =>
                ((INamedTypeSymbol)syntaxContext.TargetSymbol, (ClassDeclarationSyntax)syntaxContext.TargetNode));

        context.RegisterSourceOutput(targets, static (productionContext, target) =>
        {
            StubSourceEmitter.Emit(productionContext, target.Item1);
        });
    }
}