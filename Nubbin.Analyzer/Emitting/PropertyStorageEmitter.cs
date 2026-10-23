using System.Text;
using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Analyzer.Emitting;

internal static class PropertyStorageEmitter
{
    public const string PropertyContainerFieldName = "__Nubbin__PropertyContainer";

    public static void AppendPropertyStorage(
        this IndentedStringBuilder builder,
        StubDefinition type,
        IEnumerable<IPropertySymbol> properties)
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
                    var missingProperty = new MissingProperty(property, type);

                    if (!property.Type.CanBeInstantiated())
                    {
                        builder
                            .Append("private ")
                            .Append(PropertyEmitter.GetBackingFieldType(property.Type))
                            .Append(" ")
                            .Append(PropertyEmitter.GetBackingFieldName(missingProperty))
                            .AppendLine(";");
                    }

                    builder.Append($"public {propType} {property.Name}");
                    builder.AppendAutoPropertyBody(missingProperty, generateGetter: true, generateSetter: true);
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
                var propertyContainerTypeName = "global::" + GetStubTypeName(type);
                var requiresCast = !string.Equals(typeName, propertyContainerTypeName, StringComparison.Ordinal);

                var includeTypeParameters = ShouldIncludeTypeParameters(referenceType);

                builder
                    .Append("extension");
                if (includeTypeParameters)
                {
                    builder
                        .Append("<")
                        .Append(string.Join(", ", referenceType.TypeParameters.Select(p => p.Name)))
                        .Append(">");
                }

                builder
                    .Append("(")
                    .Append(typeName)
                    .Append(" owner)");
                if (includeTypeParameters)
                    foreach (var typeParam in referenceType.TypeParameters)
                    {
                        var constraints = typeParam.FormatConstraints();
                        if (!string.IsNullOrEmpty(constraints))
                            builder.Append(" ").Append(constraints);
                    }
                builder.AppendLine().AppendLine("{").Indent();

                builder
                    .Append("internal global::")
                    .Append(GetStubTypeName(type))
                    .Append(".")
                    .Append(GetPropertyStorageTypeName(type))
                    .Append(" Properties")
                    .AppendLine()
                    .Indent()
                    .Append("=> ");

                if (requiresCast)
                {
                    builder
                        .Append("(owner as global::")
                        .Append(GetStubTypeName(type))
                        .Append(")?.")
                        .Append(PropertyContainerFieldName)
                        .Append(" ?? new global::")
                        .Append(GetStubTypeName(type))
                        .Append(".")
                        .Append(GetPropertyStorageTypeName(type))
                        .Append("();");
                }
                else
                {
                    builder.Append("owner.").Append(PropertyContainerFieldName).Append(";");
                }

                builder.Pop().AppendLine().Pop().AppendLine("}");
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
        return $"{FormatContainingTypes(type.ContainingType)}.{type.Name}";
    }
}