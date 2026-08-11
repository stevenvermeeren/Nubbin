using Microsoft.CodeAnalysis;

internal class MethodDiscoverer : SymbolDiscoverer<IMethodSymbol>
{
    protected override bool AllowsStubbing(IMethodSymbol candidate)
    {
        return candidate.IsAbstract &&
            candidate.MethodKind is not MethodKind.PropertyGet and not MethodKind.PropertySet &&
            candidate.MethodKind is not MethodKind.EventAdd and not MethodKind.EventRemove;
    }

    protected override string GetKey(IMethodSymbol candidate)
    {
        return candidate.Name + candidate.Arity + ":" + string.Join(",", candidate.Parameters.Select(parameter =>
            parameter.RefKind + ":" + parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
    }

    protected override bool IsSameMember(IMethodSymbol existing, IMethodSymbol candidate)
    {
        return existing.Arity == candidate.Arity &&
            existing.Parameters.Length == candidate.Parameters.Length &&
            existing.Parameters.Zip(candidate.Parameters, (leftParameter, rightParameter) =>
                leftParameter.RefKind == rightParameter.RefKind &&
                SymbolEqualityComparer.Default.Equals(leftParameter.Type, rightParameter.Type)).All(result => result);
    }
 }