using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Discovery;

internal class PropertyDiscoverer : SymbolDiscoverer<IPropertySymbol>
{
    protected override bool IsSameMember(IPropertySymbol existing, IPropertySymbol candidate)
    {
        return existing.Name == candidate.Name
            && SymbolEqualityComparer.Default.Equals(existing.Type, candidate.Type);
    }

    protected override MissingMember CreateResult(StubDefinition type, IPropertySymbol candidate)
    {
        return new MissingProperty(candidate, type);
    }
}