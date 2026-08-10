using Microsoft.CodeAnalysis;

namespace Nubbin.Generator;

internal static class StubDefaults
{
    public static bool IsTask(ITypeSymbol returnType, out ITypeSymbol? resultType)
    {
        if (returnType is INamedTypeSymbol namedType &&
            namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
            namedType.Name == "Task")
        {
            resultType = namedType.TypeArguments.Length == 1 ? namedType.TypeArguments[0] : null;
            return true;
        }

        resultType = null;
        return false;
    }

    public static string GetReturnExpression(ITypeSymbol returnType, NullableAnnotation? nullableAnnotation = null)
    {
        var effectiveNullableAnnotation = nullableAnnotation ?? returnType.NullableAnnotation;
        if (returnType is INamedTypeSymbol collectionType && GetCollectionExpression(collectionType) is { } collectionExpression)
        {
            return collectionExpression;
        }

        if (returnType is INamedTypeSymbol constructibleType &&
            effectiveNullableAnnotation == NullableAnnotation.NotAnnotated &&
            HasPublicParameterlessConstructor(constructibleType))
        {
            return "new " + returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "()";
        }

        return "default!";
    }

    public static bool RequiresNotImplemented(ITypeSymbol returnType, NullableAnnotation? nullableAnnotation = null)
    {
        var effectiveNullableAnnotation = nullableAnnotation ?? returnType.NullableAnnotation;
        if (!returnType.IsReferenceType || effectiveNullableAnnotation != NullableAnnotation.NotAnnotated)
        {
            return false;
        }

        if (returnType is not INamedTypeSymbol namedType)
        {
            return true;
        }

        return !HasPublicParameterlessConstructor(namedType) && GetCollectionExpression(namedType) is null;
    }

    private static bool HasPublicParameterlessConstructor(INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class &&
            type.InstanceConstructors.Any(constructor =>
                constructor.DeclaredAccessibility == Accessibility.Public && constructor.Parameters.Length == 0);
    }

    private static string? GetCollectionExpression(INamedTypeSymbol type)
    {
        var typeName = type.ContainingNamespace.ToDisplayString() + "." + type.Name;
        var typeArguments = type.TypeArguments
            .Select(argument => argument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .ToArray();

        return typeName switch
        {
            "System.Collections.Generic.IEnumerable" when typeArguments.Length == 1 =>
                "global::System.Array.Empty<" + typeArguments[0] + ">()",
            "System.Collections.Generic.IReadOnlyCollection" when typeArguments.Length == 1 =>
                "global::System.Array.Empty<" + typeArguments[0] + ">()",
            "System.Collections.Generic.IReadOnlyList" when typeArguments.Length == 1 =>
                "global::System.Array.Empty<" + typeArguments[0] + ">()",
            "System.Collections.Generic.ICollection" when typeArguments.Length == 1 =>
                "new global::System.Collections.Generic.List<" + typeArguments[0] + ">()",
            "System.Collections.Generic.IList" when typeArguments.Length == 1 =>
                "new global::System.Collections.Generic.List<" + typeArguments[0] + ">()",
            "System.Collections.Generic.ISet" when typeArguments.Length == 1 =>
                "new global::System.Collections.Generic.HashSet<" + typeArguments[0] + ">()",
            "System.Collections.Generic.IDictionary" when typeArguments.Length == 2 =>
                "new global::System.Collections.Generic.Dictionary<" + typeArguments[0] + ", " + typeArguments[1] + ">()",
            "System.Collections.Generic.IReadOnlyDictionary" when typeArguments.Length == 2 =>
                "new global::System.Collections.Generic.Dictionary<" + typeArguments[0] + ", " + typeArguments[1] + ">()",
            "System.Collections.IEnumerable" when typeArguments.Length == 0 =>
                "global::System.Array.Empty<object>()",
            "System.Collections.ICollection" when typeArguments.Length == 0 =>
                "new global::System.Collections.ArrayList()",
            "System.Collections.IList" when typeArguments.Length == 0 =>
                "new global::System.Collections.ArrayList()",
            _ => null
        };
    }
}