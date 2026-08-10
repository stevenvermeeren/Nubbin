using Microsoft.CodeAnalysis;

namespace Nubbin.Generator;

internal static class StubMemberDiscovery
{
    public static IEnumerable<IPropertySymbol> FindMissingProperties(INamedTypeSymbol type)
    {
        var candidates = new List<IPropertySymbol>();

        for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            candidates.AddRange(baseType.GetMembers().OfType<IPropertySymbol>()
                .Where(property => property.IsAbstract && !property.IsIndexer));
        }

        foreach (var interfaceType in type.AllInterfaces)
        {
            candidates.AddRange(interfaceType.GetMembers().OfType<IPropertySymbol>()
                .Where(property => !property.IsIndexer));
        }

        var emitted = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
        {
            var key = candidate.Name + ":" + candidate.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!emitted.Add(key) || HasConcreteImplementation(type, candidate))
            {
                continue;
            }

            yield return candidate;
        }
    }

    public static IEnumerable<IMethodSymbol> FindMissingMethods(INamedTypeSymbol type)
    {
        var candidates = new List<IMethodSymbol>();

        for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            candidates.AddRange(baseType.GetMembers().OfType<IMethodSymbol>().Where(IsStubMethod));
        }

        foreach (var interfaceType in type.AllInterfaces)
        {
            candidates.AddRange(interfaceType.GetMembers().OfType<IMethodSymbol>().Where(IsStubMethod));
        }

        var emitted = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
        {
            var key = GetMethodKey(candidate);
            if (!emitted.Add(key) || HasConcreteImplementation(type, candidate))
            {
                continue;
            }

            yield return candidate;
        }
    }

    private static bool IsStubMethod(IMethodSymbol method)
    {
        return method.IsAbstract &&
            method.MethodKind is not MethodKind.PropertyGet and not MethodKind.PropertySet;
    }

    private static bool HasConcreteImplementation(INamedTypeSymbol type, IMethodSymbol candidate)
    {
        if (candidate.ContainingType.TypeKind == TypeKind.Interface)
        {
            var implementation = type.FindImplementationForInterfaceMember(candidate);
            return implementation is not null && !implementation.IsAbstract;
        }

        for (var current = type; current is not null; current = current.BaseType)
        {
            var method = current.GetMembers(candidate.Name).OfType<IMethodSymbol>()
                .FirstOrDefault(item => HasSameSignature(item, candidate));
            if (method is not null)
            {
                return !method.IsAbstract;
            }
        }

        return false;
    }

    private static bool HasConcreteImplementation(INamedTypeSymbol type, IPropertySymbol candidate)
    {
        if (candidate.ContainingType.TypeKind == TypeKind.Interface)
        {
            var implementation = type.FindImplementationForInterfaceMember(candidate);
            return implementation is not null && !implementation.IsAbstract;
        }

        for (var current = type; current is not null; current = current.BaseType)
        {
            var property = current.GetMembers(candidate.Name).OfType<IPropertySymbol>()
                .FirstOrDefault(item => SymbolEqualityComparer.Default.Equals(item.Type, candidate.Type));
            if (property is not null)
            {
                return !property.IsAbstract;
            }
        }

        return false;
    }

    private static bool HasSameSignature(IMethodSymbol left, IMethodSymbol right)
    {
        return left.Arity == right.Arity &&
            left.Parameters.Length == right.Parameters.Length &&
            left.Parameters.Zip(right.Parameters, (leftParameter, rightParameter) =>
                leftParameter.RefKind == rightParameter.RefKind &&
                SymbolEqualityComparer.Default.Equals(leftParameter.Type, rightParameter.Type)).All(result => result);
    }

    private static string GetMethodKey(IMethodSymbol method)
    {
        return method.Name + method.Arity + ":" + string.Join(",", method.Parameters.Select(parameter =>
            parameter.RefKind + ":" + parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
    }
}