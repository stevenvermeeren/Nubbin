using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Emitting;

internal static class ClassEmitter
{
    public class Definition
    {
        public Accessibility Accessibility { get; }
        public string Name { get; }
        public bool IsPartial { get; set; }
        public bool IsStatic { get; set; }
        public bool IsSealed { get; set; }
        public INamedTypeSymbol[] BaseTypes { get; set; } = [];

        public Definition(StubDefinition type) : this(type.Accessibility, type.Name)
        { }

        public Definition(INamedTypeSymbol type) : this(type.DeclaredAccessibility, type.Name)
        { }

        public Definition(Accessibility accessibility, string name)
        {
            Accessibility = accessibility;
            Name = name;
        }
    }

    public static void WithClass(
        this IndentedStringBuilder builder,
        Definition definition,
        Action emitContents)
    {
        builder.Append(definition.Accessibility.AsTypeAccessibility());
        if (definition.IsStatic)
            builder.Append(" static");
        if (definition.IsSealed)
            builder.Append(" sealed");
        if (definition.IsPartial)
            builder.Append(" partial");
        builder.Append(" class ").Append(definition.Name);

        if (definition.BaseTypes.Length > 0)
            builder
                .Append(" : ")
                .Append(string.Join(", ", definition.BaseTypes.Select(t => t.GetFullyQualifiedName())));

        builder.AppendLine();
        builder.AppendLine("{").Indent();

        emitContents();

        builder.Pop().AppendLine("}");
    }
}