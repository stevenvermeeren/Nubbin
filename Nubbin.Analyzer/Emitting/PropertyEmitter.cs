using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Discovery;
using System.Text;

namespace Nubbin.Analyzer.Emitting;

internal static class PropertyEmitter
{
    public static void AppendProperty(
        this IndentedStringBuilder source,
        MissingProperty property)
    {
        var notNull = property.Property.GetAttributes().Any(a =>
            a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullAttribute" ||
            a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute");

        if (RequiresBackingField(property, notNull))
        {
            source.Append("private ").Append(GetBackingFieldType(property.Property.Type)).Append(" ").Append(GetBackingFieldName(property)).AppendLine(";");
        }

        source.Append(GetPropertyDeclaration(property));
        if (property.RequiresPropertyStorage)
        {
            source.AppendLine();
            source.AppendLine("{").Indent();
            if (property.HasGetter())
            {
                source
                    .Append("get => ")
                    .Append(PropertyStorageEmitter.PropertyContainerFieldName)
                    .Append(".")
                    .Append(property.Property.Name)
                    .AppendLine(";");
            }
            if (property.HasSetter())
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
        source.AppendAutoPropertyBody(property, property.HasGetter(), property.HasSetter());
    }

    internal static void AppendAutoPropertyBody(this IndentedStringBuilder source, MissingProperty property, bool generateGetter, bool generateSetter)
    {
        var notNull = property.Property.GetAttributes().Any(a =>
            a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullAttribute" ||
            a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute");

        if (!property.Property.Type.CanBeInstantiated(!notNull))
        {
            source.AppendLine();
            source.AppendLine("{").Indent();

            if (property.ImplementExplicitly && property.Property.GetMethod is null)
            {
                source.AppendLine("set { }");
            }
            else if (generateGetter && !generateSetter)
            {
                source.Append("get => throw new global::System.NotImplementedException();").AppendLine();
            }
            else
            {
                if (generateGetter)
                    source.Append("get => ").Append(GetBackingFieldName(property)).AppendLine(" ?? throw new global::System.NotImplementedException();");
                if (generateSetter)
                    source.Append("set => ").Append(GetBackingFieldName(property)).AppendLine(" = value;");
            }

            source.Pop().AppendLine("}");
            return;
        }

        if (property.ImplementExplicitly &&
            (property.Property.GetMethod is null || property.Property.SetMethod is null))
        {
            source.AppendLine();
            source.AppendLine("{").Indent();
            
            if (property.Property.GetMethod is null)
            {
                source.AppendLine("set { }");
            }
            else if (property.Property.SetMethod is null)
            {
                source
                    .Append("get => ")
                    .Append(property.Property.Type.GetReturnExpression(!notNull))
                    .AppendLine(";");
            }

            source.Pop().AppendLine("}");
            return;
        }

        source
            .Append(" { get; set; } = ")
            .Append(property.Property.Type.GetReturnExpression(!notNull))
            .AppendLine(";");
    }

    private static bool RequiresBackingField(MissingProperty property, bool notNull)
    {
        if (property.RequiresPropertyStorage)
            return false;

        if (property.ImplementExplicitly && property.Property.GetMethod is null)
            return false;

        if (property.HasGetter() && !property.HasSetter())
            return false;

        return !property.Property.Type.CanBeInstantiated(!notNull);
    }

    internal static string GetBackingFieldName(MissingProperty property)
    {
        var name = property.ImplementExplicitly ? property.FormatName() : property.Property.Name;
        var builder = new StringBuilder("__Nubbin__");
        foreach (var ch in name)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        var fieldName = builder.ToString();
        return name.Length > 0 && char.IsDigit(name[0]) ? "_" + fieldName : fieldName;
    }

    internal static string GetBackingFieldType(ITypeSymbol type)
    {
        var fieldType = type.ToQualifiedString();
        if (type is ITypeParameterSymbol || (type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.NotAnnotated))
            fieldType += "?";
        return fieldType;
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

    internal static bool HasGetter(this MissingProperty property)
    {
        return property.ImplementExplicitly
            ? property.Property.GetMethod is not null
            : property.Property.ContainingType.TypeKind == TypeKind.Interface || property.Property.GetMethod is not null;
    }

    internal static bool HasSetter(this MissingProperty property)
    {
        return property.ImplementExplicitly
            ? property.Property.SetMethod is not null
            : property.Property.ContainingType.TypeKind == TypeKind.Interface || property.Property.SetMethod is not null;
    }
}