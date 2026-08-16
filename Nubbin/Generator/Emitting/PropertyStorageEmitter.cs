using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Emitting;

internal static class PropertyStorageEmitter
{
    public static void AppendPropertyStorage(
        this IndentedStringBuilder source,
        StubDefinition type,
        IReadOnlyCollection<IPropertySymbol> properties)
    {
        var typeName = type.DisplayString;
        var qualifiedPropertiesTypeName = GetQualifiedPropertyStorageTypeName(type);

        source.WithNamespace("Nubbin", () =>
        {
            source.WithClass(
                new ClassEmitter.Definition(Accessibility.Internal, "Stubs")
                {
                    IsPartial = true,
                    IsStatic = true
                },
                () =>
                {
                    source
                        .Append($"internal static {qualifiedPropertiesTypeName}")
                        .Append($" GetPropertyHelper(this {typeName} owner)")
                        .Indent();
                    source
                        .Append($"=> global::Nubbin.Internal.StubPropertyStorage")
                        .Append($"<{typeName}, {qualifiedPropertiesTypeName}>.Get(owner);")
                        .Pop();
                });
        });
        source.WithNamespace(GetPropertyHelperNamespace(type), () =>
        {
            source.WithClass(
                new ClassEmitter.Definition(Accessibility.Internal, GetPropertyStorageTypeName(type))
                {
                    IsSealed = true
                },
                () =>
                {
                    foreach (var property in properties)
                    {
                        var propType = property.Type.ToQualifiedString();                        
                        source
                            .Append($"public {propType} {property.Name}")
                            .AppendAutoPropertyBody(property);
                    }
                });
        });
    }

    private static string GetQualifiedPropertyStorageTypeName(StubDefinition type)
    {
        return "global::" + GetPropertyHelperNamespace(type) + "." + GetPropertyStorageTypeName(type);
    }

    private static string GetPropertyStorageTypeName(StubDefinition type)
    {
        return type.Name + "PropertyHelper";
    }

    private static string GetPropertyHelperNamespace(StubDefinition type)
    {
        var namespaceName = type.Namespace;
        return string.IsNullOrEmpty(namespaceName) ? "Nubbin" : namespaceName + ".Nubbin";
    }
}