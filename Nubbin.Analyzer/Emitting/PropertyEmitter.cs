using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Analyzer.Emitting;

internal static class PropertyEmitter
{
    public static void AppendProperty(
        this IndentedStringBuilder source,
        MissingProperty property)
    {
        source.Append(GetPropertyDeclaration(property));
        if (property.RequiresPropertyStorage)
        {
            source.AppendLine();
            source.AppendLine("{").Indent();
            if (property.Property.HasGetter())
            {
                source
                    .Append("get => ")
                    .Append(PropertyStorageEmitter.PropertyContainerFieldName)
                    .Append(".")
                    .Append(property.Property.Name)
                    .AppendLine(";");
            }
            if (property.Property.HasSetter())
            {
                source
                    .Append("set => ")
                    .Append(PropertyStorageEmitter.PropertyContainerFieldName)
                    .Append(".")
                    .Append(property.Property.Name)
                    .AppendLine(" = value;");
            }
            source.Pop().AppendLine("}");
        }
        else
        {
            source.AppendAutoPropertyBody(property);
        }     
    }

    public static void AppendAutoPropertyBody(this IndentedStringBuilder source, MissingProperty property)
    {
        var notNull = property.Property.GetAttributes().Any(a =>
            a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullAttribute" ||
            a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute");
        if (property.ImplementExplicitly)
        {
            if (property.Property.GetMethod is null)
            {
                source.AppendLine();
                source.AppendLine("{").Indent();
                source.AppendLine("set { }");
                source.Pop().AppendLine("}");
                return;
            }
            else if (property.Property.SetMethod is null)
            {
                source.AppendLine();
                source.AppendLine("{").Indent();
                source.Append("get => ");
                if (!property.Property.Type.CanBeInstantiated(!notNull))
                    source.AppendLine("throw new global::System.NotImplementedException();");
                else
                    source.Append(property.Property.Type.GetReturnExpression(!notNull)).AppendLine(";");
                source.Pop().AppendLine("}");
                return;
            }
        }
        if (!property.Property.Type.CanBeInstantiated(!notNull))
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
                .Append(property.Property.Type.GetReturnExpression(!notNull))
                .AppendLine(";");
        }
    }

    private static string GetPropertyDeclaration(MissingProperty property)
    {
        var propertyType = property.Property.Type.ToQualifiedString();
        var overrideModifier = property.GetOverrideModifier();
        var accessibility = property.FormatMemberAccessibility();
        var attributes = property.Property.GetAttributes().Format();
        var name = property.FormatName();

        return $"{attributes}{accessibility}{overrideModifier}{propertyType} {name}";
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
}