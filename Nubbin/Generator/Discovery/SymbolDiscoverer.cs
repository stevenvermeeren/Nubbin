using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Discovery;

internal abstract class SymbolDiscoverer<T> where T : ISymbol
{
    public IEnumerable<T> GetSymbolsMissingImplementation(StubDefinition type)
    {
        var candidates = new List<T>();

        for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            candidates.AddRange(FindCandidates(baseType));
        }

        foreach (var interfaceType in type.AllInterfaces)
        {
            candidates.AddRange(FindCandidates(interfaceType));
        }

        var emitted = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
        {
            var key = GetKey(candidate);
            if (emitted.Add(key) && !HasConcreteImplementation(type, candidate))
                yield return candidate;
        }
    }

    private IEnumerable<T> FindCandidates(INamedTypeSymbol type)
    {
        return type.GetMembers()
            .OfType<T>()
            .Where(member => member.IsAbstract)
            .Where(AllowsStubbing);
    }

    private bool HasConcreteImplementation(StubDefinition type, T candidate)
    {
        var concreteType = type.LeafType 
            ?? type.BaseType
            ?? throw new ArgumentException("No concrete type found on Stubbable");
            
        if (candidate.ContainingType.TypeKind == TypeKind.Interface)
        {
            var implementation = concreteType.FindImplementationForInterfaceMember(candidate);
            return implementation is not null && !implementation.IsAbstract;
        }

        for (var current = concreteType; current is not null; current = current.BaseType)
        {
            var member = current.GetMembers(candidate.Name).OfType<T>()
                .FirstOrDefault(item => IsSameMember(item, candidate));
            if (member is not null)
            {
                return !member.IsAbstract;
            }
        }

        return false;
    }

    protected virtual bool AllowsStubbing(T candidate)
    {
        return true;
    }

    protected abstract string GetKey(T candidate);

    protected abstract bool IsSameMember(T existing, T candidate);
}