using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Emitting;

internal static class ClassEmitter
{
    public class Definition
    {
        public Accessibility Accessibility { get; }
        public string Name { get; }
        public bool IsPartial { get; set; }
        public bool IsStatic { get; set; }
        public bool IsSealed { get; set; }
        public IReadOnlyList<ITypeParameterSymbol> TypeParameters { get; set; } = [];
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

        if (definition.TypeParameters.Any())
            builder
                .Append("<")
                .Append(string.Join(", ", definition.TypeParameters.Select(p => p.Name)))
                .Append(">");

        if (definition.BaseTypes.Length > 0)
            builder
                .Append(" : ")
                .Append(string.Join(", ", definition.BaseTypes.Select(t => t.GetFullyQualifiedName())));

        builder.AppendLine();
        foreach (var typeParam in definition.TypeParameters)
            if (typeParam.FormatConstraints() is { Length: > 0 } constraint)
                builder.Indent().AppendLine(constraint).Pop();
        builder.AppendLine("{").Indent();

        emitContents();

        builder.Pop().AppendLine("}");
    }
}