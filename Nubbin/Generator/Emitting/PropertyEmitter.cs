using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Emitting;

internal static class PropertyEmitter
{
    public static void AppendProperty(
        this IndentedStringBuilder source,
        StubDefinition type,
        IPropertySymbol property,
        bool useStorage)
    {
        source.Append(type.GetPropertyDeclaration(property));
        if (useStorage)
        {
            source.AppendLine();
            source.AppendLine("{").Indent();
            if (property.HasGetter())
            {
                source
                    .Append("get => global::Nubbin.Stubs.GetPropertyHelper(this).")
                    .Append(property.Name)
                    .AppendLine(";");
            }
            if (property.HasSetter())
            {
                source
                    .Append("set => global::Nubbin.Stubs.GetPropertyHelper(this).")
                    .Append(property.Name)
                    .AppendLine(" = value;");
            }
            source.AppendLine("}");
        }
        else
        {
            source.AppendAutoPropertyBody(property);
        }     
    }

    public static void AppendAutoPropertyBody(this IndentedStringBuilder source, IPropertySymbol property)
    {
        if (property.Type.RequiresNotImplemented(property.NullableAnnotation))
        {
            source.AppendLine();
            source.AppendLine("{").Indent();
            source.AppendLine("get => throw new global::System.NotImplementedException();");
            source.AppendLine("set { }");
            source.Pop().AppendLine("}");
        }
        else
        {
            source
                .Append(" { get; set; } = ")
                .Append(property.Type.GetReturnExpression(property.NullableAnnotation))
                .AppendLine(";");
        }
    }

    private static string GetPropertyDeclaration(this StubDefinition type, IPropertySymbol property)
    {
        var propertyType = property.Type.ToQualifiedString();
        var overrideModifier = property.ContainingType.TypeKind == TypeKind.Interface
            ? string.Empty
            : "override ";
        var accessibility = property.GetMemberAccessibility(type.ContainingAssembly);

        return $"{accessibility} {overrideModifier}{propertyType} {property.Name}";
    }

    private static bool HasGetter(this IPropertySymbol property)
    {
        return property.ContainingType.TypeKind == TypeKind.Interface
            || property.GetMethod is not null;
    }

    private static bool HasSetter(this IPropertySymbol property)
    {
        return property.ContainingType.TypeKind == TypeKind.Interface 
            || property.SetMethod is not null;
    }

    public static bool RequiresPropertyStorage(IPropertySymbol property)
    {
        return property.ContainingType.TypeKind != TypeKind.Interface
            && (property.GetMethod is null) != (property.SetMethod is null);
    }
}