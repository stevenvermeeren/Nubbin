using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Nubbin.Generator.Emitting;

namespace Nubbin.Generator;

internal class StubDefinition
{
    public string Name { get; set; } = "";
    public string Namespace { get; set; } = "";
    public IAssemblySymbol ContainingAssembly { get; set; } = null!;
    public Accessibility Accessibility { get; set; }
    public INamedTypeSymbol? LeafType { get; set; }
    public INamedTypeSymbol? BaseType { get; set; }
    public INamedTypeSymbol? ContainingType { get; set; }
    public ImmutableArray<INamedTypeSymbol> AllInterfaces { get; set; } = [];

    public static StubDefinition FromINamedTypeSymbol(INamedTypeSymbol symbol)
    {
        return new StubDefinition
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            Accessibility = symbol.DeclaredAccessibility,
            ContainingAssembly = symbol.ContainingAssembly,
            ContainingType = symbol.ContainingType,
            BaseType = symbol.BaseType,
            LeafType = symbol,
            AllInterfaces = symbol.AllInterfaces
        };
    }
}
