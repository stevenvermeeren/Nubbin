using System.Text;
using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Emitting;

internal static class PropertyStorageEmitter
{
    public const string PropertyContainerFieldName = "__Nubbin__PropertyContainer";

    public static void AppendPropertyStorage(
        this IndentedStringBuilder builder,
        StubDefinition type,
        IReadOnlyCollection<IPropertySymbol> properties)
    {
        builder.AppendLine("#pragma warning disable CS0108 // Potentially hiding member");
        builder
            .Append("internal readonly ")
            .Append(GetPropertyStorageTypeName(type))
            .Append(" ")
            .Append(PropertyContainerFieldName)
            .AppendLine(" = new();");
        builder.AppendLine("#pragma warning restore CS0108 // Potentially hiding member");

        builder.WithClass(
            new ClassEmitter.Definition(Accessibility.Internal, GetPropertyStorageTypeName(type))
            {
                IsSealed = true
            },
            () =>
            {
                foreach (var property in properties)
                {
                    var propType = property.Type.ToQualifiedString();                        
                    builder
                        .Append($"public {propType} {property.Name}")
                        .AppendAutoPropertyBody(property);
                }
            });
    }

    public static void AppendPropertyStorageLookup(
        this IndentedStringBuilder builder,
        StubDefinition type)
    {
        builder.WithClass(
            new ClassEmitter.Definition(Accessibility.Internal, "Stubs")
            {
                IsPartial = true,
                IsStatic = true
            },
            () =>
            {
                var referenceType = type.LeafType ?? type.BaseType;
                var typeName = referenceType?.GetFullyQualifiedNameWithTypeParams()
                    ?? throw new InvalidOperationException("Unexpected property container for non-concrete type.");
                builder
                    .AppendLine("/// <summary>")
                    .AppendLine("/// Gets the property storage associated with a stub instance.")
                    .AppendLine("/// </summary>")
                    .AppendLine("/// <param name=\"owner\">The stub instance that owns the properties.</param>")
                    .AppendLine("/// <returns>The typed property storage for <paramref name=\"owner\"/>.</returns>");
                builder
                    .Append("internal static global::")
                    .Append(GetStubTypeName(type))
                    .Append(".")
                    .Append(GetPropertyStorageTypeName(type))
                    .Append(" GetPropertyHelper")
                    .Append(ShouldIncludeTypeParameters(referenceType) ? referenceType.FormatTypeParams() : "")
                    .Append("(this ")
                    .Append(typeName)
                    .AppendLine(" owner)")
                    .Indent();
                if (ShouldIncludeTypeParameters(referenceType))
                    foreach (var typeParam in referenceType.TypeParameters)
                        builder.Indent().AppendLine(typeParam.FormatConstraints()).Pop();
                builder
                    .Append("=> (owner as global::")
                    .Append(GetStubTypeName(type))
                    .Append(")?.")
                    .Append(PropertyContainerFieldName)
                    .Append(" ?? new global::")
                    .Append(GetStubTypeName(type))
                    .Append(".")
                    .Append(GetPropertyStorageTypeName(type))
                    .Append("();")
                    .Pop();
            });
    }

    private static string GetPropertyStorageTypeName(StubDefinition type)
    {
        return type.Name + "PropertyContainer";
    }

    private static string GetStubTypeName(StubDefinition type)
    {
        var builder = new StringBuilder();
        if(type.Namespace.Length > 0)
            builder.Append(type.Namespace).Append(".");
        if (type.ContainingType is not null)
            builder.Append(FormatContainingTypes(type.ContainingType)).Append(".");
        builder.Append(type.Name);
        if (type.LeafType is not null && ShouldIncludeTypeParameters(type.LeafType))
            builder.Append(type.LeafType.FormatTypeParams());
        return builder.ToString();
    }

    private static bool ShouldIncludeTypeParameters(INamedTypeSymbol type)
    {
        return type.TypeArguments.Any(a => a.Kind == SymbolKind.TypeParameter);
    }

    private static string FormatContainingTypes(ITypeSymbol type)
    {
        if (type.ContainingType is null)
            return type.Name;
        return $"{FormatContainingTypes(type)}.{type.Name}";
    }
}