using Microsoft.CodeAnalysis;

namespace Nubbin.Analyzer.Emitting;

internal static class MethodEmitter
{
    public static void AppendMethod(this IndentedStringBuilder source, IMethodSymbol method, StubDefinition type)
    {
        source.AppendLine(type.GetMethodDeclaration(method));
        if (method.ContainingType.TypeKind == TypeKind.Interface)
            source.AppendTypeConstraints(method.TypeParameters);
        source.AppendLine("{").Indent();

        if (CanImplement(method))
        {
            foreach (var parameter in method.Parameters.Where(parameter => parameter.RefKind == RefKind.Out))
            {
                source.Append(parameter.Name).Append(" = ")
                    .Append(StubDefaults.GetReturnExpression(parameter.Type))
                    .AppendLine(";");
            }

            if (!method.ReturnsVoid)
            {
                source.Append("return ").Append(StubDefaults.GetReturnExpression(method.ReturnType)).AppendLine(";");
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

    private static string GetMethodDeclaration(this StubDefinition type, IMethodSymbol method)
    {
        var returnType = method.ReturnType.ToQualifiedString();
        var parameters = string.Join(", ", method.Parameters.Select(parameter =>
            parameter.GetAttributes().Format() +
            (parameter.IsParams ? "params " : string.Empty) +
            (parameter.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty
            }) + parameter.Type.ToQualifiedString() + " " + parameter.Name));
        var typeParameters = method.Arity == 0 ? string.Empty : "<" + string.Join(", ", method.TypeParameters.Select(parameter => parameter.ToQualifiedString())) + ">";
        var overrideModifier = method.ContainingType.TypeKind == TypeKind.Interface ? string.Empty : "override ";
        var accessibility = method.GetMemberAccessibility(type.ContainingAssembly);

        return $"{accessibility} {overrideModifier}{returnType} {method.Name}{typeParameters}({parameters})";
    }
}