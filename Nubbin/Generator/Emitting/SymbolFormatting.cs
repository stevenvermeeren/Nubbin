using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nubbin.Generator.Emitting;

internal static class SymbolFormatting
{
    public static string GetStubTypeName(this TypeSyntax type)
    {
        return type.ToFullString() + "Stub";
    }

    public static string ToQualifiedString(this ITypeSymbol type)
    {
        var name = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            name += "?";
        return name;
    }

    public static string GetFullyQualifiedName(INamespaceSymbol _namespace, TypeSyntax type)
    { 
        if (_namespace.IsGlobalNamespace)
            return $"global::{type.ToFullString()}";
        return $"global::{_namespace.ToDisplayString()}.{type.ToFullString()}";
    }

    public static string GetFullyQualifiedName(this INamedTypeSymbol symbol)
    {
        if (symbol.ContainingNamespace.IsGlobalNamespace)
            return $"global::{symbol.Name}";
        return $"global::{symbol.ContainingNamespace.ToDisplayString()}.{symbol.Name}";
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