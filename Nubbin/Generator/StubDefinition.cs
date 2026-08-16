using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Nubbin.Generator;

internal class StubDefinition
{
    public string Name { get; set; } = "";
    public string DisplayString { get; set; } = "";
    public string Namespace { get; set; } = "";
    public IAssemblySymbol ContainingAssembly { get; set; } = null!;
    public Accessibility Accessibility { get; set; }
    public INamedTypeSymbol? LeafType { get; set; }
    public INamedTypeSymbol? BaseType { get; set; }
    public ImmutableArray<INamedTypeSymbol> AllInterfaces { get; set; } = [];

    public static StubDefinition FromINamedTypeSymbol(INamedTypeSymbol symbol)
    {
        return new StubDefinition
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            Accessibility = symbol.DeclaredAccessibility,
            ContainingAssembly = symbol.ContainingAssembly,
            BaseType = symbol.BaseType,
            LeafType = symbol,
            AllInterfaces = symbol.AllInterfaces,
            DisplayString = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        };
    }
}
