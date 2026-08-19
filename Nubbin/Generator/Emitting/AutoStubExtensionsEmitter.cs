using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nubbin.Generator.Emitting;

internal static class AutoStubExtensionsEmitter
{
    public static string Emit(IEnumerable<INamedTypeSymbol> stubs)
    {
        var builder = new IndentedStringBuilder();
        builder.WithNamespace("Nubbin", () =>
        {
            builder.WithClass(new ClassEmitter.Definition(Accessibility.Internal, "AutoStubExtensions_Generated")
                { IsStatic = true }, () =>
            {
                builder.AppendLine("extension(global::Nubbin.Stub)");
                builder.AppendLine("{").Indent();
                
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
        INamedTypeSymbol stubTypeSymbol)
    {
        builder
            .AppendLine($"if (typeof(T) == typeof({stubTypeSymbol.GetFullyQualifiedName()}))")
            .Indent()
            .AppendLine($"return (T)(object)new global::Nubbin.Generated.{stubTypeSymbol.GetStubTypeNameWithNamespace()}();")
            .Pop();
    }

    private static INamedTypeSymbol GetStubTypeSymbol(GeneratorSyntaxContext syntaxContext, MemberAccessExpressionSyntax stub)
    {
        var stubType = ((GenericNameSyntax)stub.Name).TypeArgumentList.Arguments.Single();
        var typeInfo = syntaxContext.SemanticModel.GetTypeInfo(stubType);
        var stubTypeSymbol = (INamedTypeSymbol)typeInfo.Type!;
        return stubTypeSymbol;
    }
}