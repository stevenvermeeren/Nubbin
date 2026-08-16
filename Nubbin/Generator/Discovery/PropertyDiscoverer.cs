using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Discovery;

internal class PropertyDiscoverer : SymbolDiscoverer<IPropertySymbol>
{
    protected override string GetKey(IPropertySymbol candidate)
    {
        return $"{candidate.Name}:{candidate.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";
    }

    protected override bool IsSameMember(IPropertySymbol existing, IPropertySymbol candidate)
    {
        return existing.Name == candidate.Name
            && SymbolEqualityComparer.Default.Equals(existing.Type, candidate.Type);
    }
}