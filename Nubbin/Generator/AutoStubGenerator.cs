using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nubbin.Generator.Emitting;

namespace Nubbin.Generator;

/// <summary>
/// Generates dummy implementations for classes referenced by <see cref="Stub.Auto{T}"/>. 
/// </summary>
[Generator]
public sealed class AutoStubGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Registers the incremental pipeline that discovers <see cref="Stub.Auto{T}"/> targets.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is CompilationUnitSyntax,
            static (syntaxContext, _) => syntaxContext);

        context.RegisterSourceOutput(targets, static (productionContext, syntaxContext) =>
        {
            var unit = (CompilationUnitSyntax)syntaxContext.Node;
            var generator = new AutoStubFactory(productionContext, syntaxContext);
            generator.Generate(unit);
        });
    }
}