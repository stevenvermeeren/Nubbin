using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nubbin.Generator.Emitting;

internal static class AutoStubContainerEmitter
{
    public static string Emit(
        GeneratorSyntaxContext syntaxContext,
        INamedTypeSymbol containerType,
        IEnumerable<MemberAccessExpressionSyntax> stubs)
    {
        var builder = new IndentedStringBuilder();
        builder.WithNamespace(containerType.ContainingNamespace.ToDisplayString(), () =>
        {
            builder.WithClass(new ClassEmitter.Definition(containerType) { IsPartial = true }, () =>
            {
                builder.WithClass(new ClassEmitter.Definition(Accessibility.Private, "Stub") { IsStatic = true }, () =>
                {
                    builder.AppendLine("public static T Auto<T>() {").Indent();

                    foreach (var stub in stubs)
                    {
                        builder.AppendStubLookup(syntaxContext, stub);
                    }
                    builder.AppendLine("throw new NotSupportedException($\"No stub found for type {typeof(T)}\");");
                    
                    builder.Pop().AppendLine("}");
                });
            });
        });
        
        return builder.ToString();
    }

    private static void AppendStubLookup(
        this IndentedStringBuilder builder,
        GeneratorSyntaxContext syntaxContext,
        MemberAccessExpressionSyntax stub)
    {
        var stubType = ((GenericNameSyntax)stub.Name).TypeArgumentList.Arguments.Single();
        var typeInfo = syntaxContext.SemanticModel.GetTypeInfo(stubType);
        var stubTypeSymbol = (INamedTypeSymbol)typeInfo.Type!;
        builder
            .AppendLine($"if (typeof(T) == typeof({stubTypeSymbol.GetFullyQualifiedName()}))")
            .Indent()
            .AppendLine($"return (T)(object)new global::Nubbin.Generated.{stubTypeSymbol.GetStubTypeNameWithNamespace()}();")
            .Pop();
    }
}