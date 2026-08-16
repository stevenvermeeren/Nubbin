using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Discovery;

internal class EventDiscoverer : SymbolDiscoverer<IEventSymbol>
{
    protected override string GetKey(IEventSymbol candidate)
    {
        return $"{candidate.Name}:{candidate.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";
    }

    protected override bool IsSameMember(IEventSymbol existing, IEventSymbol candidate)
    {
        return existing.Name == candidate.Name
            && SymbolEqualityComparer.Default.Equals(existing.Type, candidate.Type);
    }
}