using Microsoft.CodeAnalysis;

namespace Nubbin.Generator;

internal static class SymbolFormatting
{
    public static string GetMethodDeclaration(IMethodSymbol method)
    {
        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var parameters = string.Join(", ", method.Parameters.Select(parameter =>
            (parameter.IsParams ? "params " : string.Empty) +
            (parameter.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty
            }) + parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " " + parameter.Name));
        var typeParameters = method.Arity == 0 ? string.Empty : "<" + string.Join(", ", method.TypeParameters.Select(parameter => parameter.Name)) + ">";
        var overrideModifier = method.ContainingType.TypeKind == TypeKind.Interface ? string.Empty : "override ";

        return GetMemberAccessibility(method.DeclaredAccessibility, method.ContainingType.TypeKind == TypeKind.Interface) +
            " " + overrideModifier + returnType + " " + method.Name + typeParameters + "(" + parameters + ")";
    }

    public static string GetPropertyDeclaration(IPropertySymbol property)
    {
        var propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var overrideModifier = property.ContainingType.TypeKind == TypeKind.Interface ? string.Empty : "override ";

        return GetMemberAccessibility(property.DeclaredAccessibility, property.ContainingType.TypeKind == TypeKind.Interface) +
            " " + overrideModifier + propertyType + " " + property.Name;
    }

    public static bool HasGetter(IPropertySymbol property)
    {
        return property.ContainingType.TypeKind == TypeKind.Interface || property.GetMethod is not null;
    }

    public static bool HasSetter(IPropertySymbol property)
    {
        return property.ContainingType.TypeKind == TypeKind.Interface || property.SetMethod is not null;
    }

    public static string GetTypeAccessibility(INamedTypeSymbol type)
    {
        return type.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            _ => string.Empty
        };
    }

    private static string GetMemberAccessibility(Accessibility accessibility, bool isInterfaceMember)
    {
        if (isInterfaceMember)
        {
            return "public";
        }

        return accessibility switch
        {
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "public"
        };
    }
}