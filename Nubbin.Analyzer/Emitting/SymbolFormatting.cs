using System.Text;
using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Emitting;

internal static class SymbolFormatting
{
    public static string GetStubTypeName(this INamedTypeSymbol type)
    {
        return WithParentTypes(type, '_') + FormatTypeParamsForName(type) + "Stub";
    }

    public static string GetStubTypeNameWithNamespace(this INamedTypeSymbol type)
    {
        return type.GetFullyQualifiedName(false, '_') + FormatTypeParamsForName(type) + "Stub";
    }

    private static string FormatTypeParamsForName(INamedTypeSymbol type) =>
        type.TypeArguments.Any()
            ? "_" + string.Join("_", type.TypeArguments.OfType<INamedTypeSymbol>().Select(a => a.Name + FormatTypeParamsForName(a)))
            : "";

    private static readonly SymbolDisplayFormat FullyQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static string ToQualifiedString(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol nts && type.SpecialType != SpecialType.System_Void)
        {
            // custom logic to ensure ? on types define without nullability awareness
            var res = $"{GetFullyQualifiedName(nts)}{FormatTypeParams(nts)}";
            if (nts.NullableAnnotation != NullableAnnotation.NotAnnotated && !nts.IsValueType)
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

    public static string GetFullyQualifiedNameWithTypeParams(
        this INamedTypeSymbol symbol,
        bool includeGlobalPrefix = true,
        char parentTypeSeparator = '.')
    {
        return symbol.GetFullyQualifiedName(includeGlobalPrefix, parentTypeSeparator)
            + symbol.FormatTypeParams();
    }

    public static string FormatTypeParams(this INamedTypeSymbol? symbol)
    {
        if (symbol is null || !symbol.TypeArguments.Any())
            return string.Empty;
        return "<" + string.Join(", ", symbol.TypeArguments.Select(p => 
            p.ContainingNamespace is null
                ? p.Name
                : p.ToQualifiedString())) + ">";
    }

    public static string FormatConstraints(this ITypeParameterSymbol symbol)
    {
        var builder = new StringBuilder("where ");
        builder.Append(symbol.Name + " : ");
        var baseLenght = builder.Length;

        foreach (var type in symbol.ConstraintTypes.OfType<INamedTypeSymbol>())
            builder.Append(type.Name).Append(", ");
        if (symbol.HasReferenceTypeConstraint)
            builder.Append("class, ");
        if (symbol.HasNotNullConstraint)
            builder.Append("notnull, ");
        if (symbol.HasValueTypeConstraint)
            builder.Append("struct, ");
        if (symbol.HasConstructorConstraint)
            builder.Append("new(), ");
        var res = builder.ToString();
        if (res.Length > baseLenght)
            return res.Substring(0, res.Length - 2);
        return string.Empty;
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