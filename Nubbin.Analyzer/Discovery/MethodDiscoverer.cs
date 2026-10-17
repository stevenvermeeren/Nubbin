using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Discovery;

internal class MethodDiscoverer : SymbolDiscoverer<IMethodSymbol>
{
    protected override bool AllowsStubbing(IMethodSymbol candidate)
    {
        return candidate.IsAbstract
            && candidate.MethodKind is not MethodKind.PropertyGet and not MethodKind.PropertySet
            && candidate.MethodKind is not MethodKind.EventAdd and not MethodKind.EventRemove;
    }

    protected override bool IsSameMember(IMethodSymbol existing, IMethodSymbol candidate)
    {
        return existing.Arity == candidate.Arity &&
            existing.Parameters.Length == candidate.Parameters.Length &&
            existing.Parameters
                .Zip(candidate.Parameters, (leftParameter, rightParameter) =>
                    leftParameter.RefKind == rightParameter.RefKind &&
                    SymbolEqualityComparer.Default.Equals(leftParameter.Type, rightParameter.Type))
                .All(result => result);
    }

    protected override MissingMember CreateResult(StubDefinition type, IMethodSymbol candidate)
    {
        return new MissingMethod(candidate, type);
    }
 }