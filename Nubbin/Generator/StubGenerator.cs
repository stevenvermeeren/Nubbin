using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nubbin.Generator.Emitting;

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
            static (syntaxContext, _) => syntaxContext);

        context.RegisterSourceOutput(targets, static (productionContext, syntaxContext) =>
        {
            var target = (INamedTypeSymbol)syntaxContext.TargetSymbol;
            if (!productionContext.CheckPartial(
                    target, 
                    () => GetAttributeFromClass(syntaxContext).GetLocation(),
                    Diagnostics.PartialStubError))
                return;

            var source = StubSourceEmitter.Emit(target);
            productionContext.AddSource(target.Name + ".Stub.g.cs", source);
        });
    }

    private static AttributeSyntax GetAttributeFromClass(GeneratorAttributeSyntaxContext context)
    {
        return context.TargetNode
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .First(n => context.SemanticModel.GetTypeInfo(n).Type?.Name == "StubAttribute");
    }
}