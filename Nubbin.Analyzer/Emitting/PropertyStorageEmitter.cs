using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Emitting;

internal static class PropertyStorageEmitter
{
    public const string PropertyContainerFieldName = "_nubbinPropertyContainer";

    public static void AppendPropertyStorage(
        this IndentedStringBuilder builder,
        StubDefinition type,
        IReadOnlyCollection<IPropertySymbol> properties)
    {
        builder
            .Append("internal readonly ")
            .Append(GetPropertyStorageTypeName(type))
            .Append(" ")
            .Append(PropertyContainerFieldName)
            .AppendLine(" = new();");
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
                var typeName = (type.LeafType ?? type.BaseType)?.GetFullyQualifiedName()
                    ?? throw new InvalidOperationException("Unexpected property container for non-concrete type.");
                builder
                    .AppendLine("/// <summary>")
                    .AppendLine("/// Gets the property storage associated with a stub instance.")
                    .AppendLine("/// </summary>")
                    .AppendLine("/// <param name=\"owner\">The stub instance that owns the properties.</param>")
                    .AppendLine("/// <returns>The typed property storage for <paramref name=\"owner\"/>.</returns>");
                builder
                    .Append("internal static ")
                    .Append(typeName)
                    .Append(".")
                    .Append(GetPropertyStorageTypeName(type))
                    .Append(" GetPropertyHelper(this ")
                    .Append(typeName)
                    .AppendLine(" owner)")
                    .Indent();
                builder
                    .Append("=> (owner as ")
                    .Append(type.Name)
                    .Append(")?.")
                    .Append(PropertyContainerFieldName)
                    .Append(" ?? new ")
                    .Append(typeName)
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
}