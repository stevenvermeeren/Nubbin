using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Analyzer.Emitting;

internal static class MethodEmitter
{
    public static void AppendMethod(this IndentedStringBuilder source, MissingMethod method)
    {
        source.AppendLine(GetMethodDeclaration(method));
        if (method.Method.ContainingType.TypeKind == TypeKind.Interface)
            source.AppendTypeConstraints(method.Method.TypeParameters);
        source.AppendLine("{").Indent();

        if (CanImplement(method.Method))
        {
            foreach (var parameter in method.Method.Parameters.Where(parameter => parameter.RefKind == RefKind.Out))
            {
                source.Append(parameter.Name).Append(" = ")
                    .Append(StubDefaults.GetReturnExpression(parameter.Type))
                    .AppendLine(";");
            }

            if (!method.Method.ReturnsVoid)
            {
                source.Append("return ").Append(StubDefaults.GetReturnExpression(method.Method.ReturnType)).AppendLine(";");
            }
        }
        else
        {
            source.AppendLine("throw new global::System.NotImplementedException();");
        }

        source.Pop().AppendLine("}");
    }

    private static bool CanImplement(IMethodSymbol method)
    {
        if (!method.ReturnType.CanBeInstantiated())
            return false;

        foreach (var param in method.Parameters)
        {
            if (param.RefKind == RefKind.Out)
            {
                if (!param.Type.CanBeInstantiated())
                    return false;
            }

            if (param.GetAttributes().Any(a =>
                    a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullAttribute"
                    || (a.AttributeClass?.GetFullyQualifiedName(false) == "System.Diagnostics.CodeAnalysis.NotNullWhenAttribute"
                        && false.Equals(a.ConstructorArguments.FirstOrDefault().Value))))
                return false;
        }
        return true;
    }

    private static string GetMethodDeclaration(MissingMethod method)
    {
        var returnType = method.Method.ReturnType.ToQualifiedString();
        var parameters = string.Join(", ", method.Method.Parameters.Select(parameter =>
            parameter.GetAttributes().Format() +
            (parameter.IsParams ? "params " : string.Empty) +
            (parameter.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty
            }) + parameter.Type.ToQualifiedString() + " " + parameter.Name));
        var typeParameters = method.Method.Arity == 0 ? string.Empty : "<" + string.Join(", ", method.Method.TypeParameters.Select(parameter => parameter.ToQualifiedString())) + ">";
        var overrideModifier = method.GetOverrideModifier();
        var accessibility = method.FormatMemberAccessibility();
        var name = method.FormatName();

        return $"{accessibility}{overrideModifier}{returnType} {name}{typeParameters}({parameters})";
    }
}