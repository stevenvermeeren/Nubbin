namespace Nubbin.Generator.Emitting;

internal static class NamespaceEmitter
{
    public static void WithNamespace(
        this IndentedStringBuilder builder,
        string _namespace,
        Action emitContents)
    {
        builder.Append("namespace ").AppendLine(_namespace);
        builder.AppendLine("{").Indent();
        emitContents();
        builder.Pop().AppendLine("}");
    }
}