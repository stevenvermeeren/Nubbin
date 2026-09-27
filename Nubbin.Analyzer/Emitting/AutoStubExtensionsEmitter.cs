using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nubbin.Analyzer.Emitting;

internal static class AutoStubExtensionsEmitter
{
    public static string Emit(IEnumerable<AutoStubSource> stubs)
    {
        var builder = new IndentedStringBuilder();
        builder.WithNamespace("Nubbin", () =>
        {
            builder.WithClass(new ClassEmitter.Definition(Accessibility.Internal, "AutoStubExtensions_Generated")
                { IsStatic = true }, () =>
            {
                builder.AppendLine("extension(global::Nubbin.Stub)");
                builder.AppendLine("{").Indent();
                
                builder.AppendLine("/// <summary>");
                builder.AppendLine("/// Method to retrieve auto-implemented stub instances.");
                builder.AppendLine("/// </summary>");
                builder
                    .AppendLine("public static T Auto<T>()")
                    .AppendLine("{")
                    .Indent();

                foreach (var stub in stubs)
                {
                    builder.AppendStubLookup(stub);
                }
                builder.AppendLine("throw new NotSupportedException($\"No stub found for type {typeof(T)}\");");
                
                builder.Pop().AppendLine("}");
                builder.Pop().AppendLine("}");
            });
        });
        
        return builder.ToString();
    }

    private static void AppendStubLookup(
        this IndentedStringBuilder builder,
        AutoStubSource stubTypeSymbol)
    {
        builder
            .Append("if (typeof(T) == typeof(")
            .Append(stubTypeSymbol.GetFullyQualifiedName())
            .AppendLine("))")
            .Indent();
        builder
            .Append("return (T)(object)")
            .Append(stubTypeSymbol.GetInstantiationExpression())
            .AppendLine(";")
            .Pop();
    }
}