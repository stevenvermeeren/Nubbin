using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nubbin.Generator.Emitting;

namespace Nubbin.Generator;

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
/// <summary>
/// Generates dummy implementations for classes referenced by <see cref="Stub.Auto{T}"/>. 
/// </summary>
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
[Generator]
public sealed class AutoStubGenerator : IIncrementalGenerator
{

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
    /// <summary>
    /// Registers the incremental pipeline that discovers <see cref="Stub.Auto{T}"/> targets.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is CompilationUnitSyntax,
            static (syntaxContext, _) => syntaxContext).Collect();

        context.RegisterSourceOutput(targets, (productionContext, syntaxContexts) =>
        {
            var generatedStubs = new Dictionary<string, INamedTypeSymbol>();
            foreach (var syntaxContext in syntaxContexts)
            {
                var unit = (CompilationUnitSyntax)syntaxContext.Node;
                var generator = new AutoStubFactory(productionContext, syntaxContext, generatedStubs);
                generator.Process(unit);
            }

            var extensions = AutoStubExtensionsEmitter.Emit(generatedStubs.Values);
            productionContext.AddSource($"Nubbin.AutoStubExtensions.g.cs", extensions.ToString());
        });
    }
}