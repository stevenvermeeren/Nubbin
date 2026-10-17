using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Discovery;

internal class EventDiscoverer : SymbolDiscoverer<IEventSymbol>
{
    protected override bool IsSameMember(IEventSymbol existing, IEventSymbol candidate)
    {
        return existing.Name == candidate.Name
            && SymbolEqualityComparer.Default.Equals(existing.Type, candidate.Type);
    }

    protected override MissingMember CreateResult(StubDefinition type, IEventSymbol candidate)
    {
        return new MissingEvent(candidate, type);
    }
}