using System.Text;
using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Emitting;

internal static class SymbolFormatting
{
    public static string GetStubTypeName(this INamedTypeSymbol type)
    {
        return WithParentTypes(type, '_') + "Stub";
    }

    public static string GetStubTypeNameWithNamespace(this INamedTypeSymbol type)
    {
        return type.GetFullyQualifiedName(false, '_') + "Stub";
    }

    private static readonly SymbolDisplayFormat FullyQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static string ToQualifiedString(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol nts && type.SpecialType != SpecialType.System_Void)
        {
            // custom logic to ensure ? on types define without nullability awareness
            var res = $"{GetFullyQualifiedName(nts)}";
            if (nts.IsGenericType)
            {
                res += "<";
                res += string.Join(", ", nts.TypeArguments.Select(a => a.ToQualifiedString()));
                res += ">";
            }
            if (nts.NullableAnnotation != NullableAnnotation.NotAnnotated)
                res += "?";
            return res;
        }

        return type.ToDisplayString(FullyQualifiedFormat);
    }

    public static string GetFullyQualifiedName(
        this INamedTypeSymbol symbol,
        bool includeGlobalPrefix = true,
        char parentTypeSeparator = '.')
    {
        if (symbol.SpecialType != SpecialType.None)
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        var result = new StringBuilder(includeGlobalPrefix ? "global::" : "");
        if (!symbol.ContainingNamespace.IsGlobalNamespace)
            result.Append($"{symbol.ContainingNamespace.ToDisplayString()}.");
        result.Append(WithParentTypes(symbol, parentTypeSeparator));
        return result.ToString();
    }

    private static string WithParentTypes(INamedTypeSymbol type, char parentTypeSeparator)
    {
        if (type.ContainingType is INamedTypeSymbol parent)
            return $"{WithParentTypes(parent, parentTypeSeparator)}{parentTypeSeparator}{type.Name}";
        return type.Name;
    }

    public static string AsTypeAccessibility(this Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            _ => string.Empty
        };
    }

    public static string GetMemberAccessibility(
        this ISymbol symbol,
        IAssemblySymbol targetAssembly)
    {
        if (symbol.ContainingType.TypeKind == TypeKind.Interface)
        {
            return "public";
        }

        return symbol.DeclaredAccessibility switch
        {
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal =>
                SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, targetAssembly)
                    ? "protected internal"
                    : "protected",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "public"
        };
    }
}