using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Emitting;

internal static class MethodEmitter
{
    public static void AppendMethod(this IndentedStringBuilder source, IMethodSymbol method, StubDefinition type)
    {
        source.AppendLine(type.GetMethodDeclaration(method));
        source.AppendLine("{").Indent();
        foreach (var parameter in method.Parameters.Where(parameter => parameter.RefKind == RefKind.Out))
        {
            if (StubDefaults.IsTask(parameter.Type, out var outTaskResultType))
            {
                if (outTaskResultType is null)
                {
                    source
                        .Append(parameter.Name)
                        .AppendLine(" = global::System.Threading.Tasks.Task.CompletedTask;");
                }
                else if (StubDefaults.RequiresNotImplemented(outTaskResultType))
                {
                    source.AppendLine("throw new global::System.NotImplementedException();");
                }
                else
                {
                    source
                        .Append(parameter.Name)
                        .Append(" = global::System.Threading.Tasks.Task.FromResult<")
                        .Append(outTaskResultType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                        .Append(">(")
                        .Append(StubDefaults.GetReturnExpression(outTaskResultType))
                        .AppendLine(");");
                }
            }
            else if (StubDefaults.RequiresNotImplemented(parameter.Type, parameter.NullableAnnotation))
            {
                source.AppendLine("throw new global::System.NotImplementedException();");
            }
            else
            {
                source.Append(parameter.Name).Append(" = ")
                    .Append(StubDefaults.GetReturnExpression(parameter.Type, parameter.NullableAnnotation))
                    .AppendLine(";");
            }
        }

        if (StubDefaults.IsTask(method.ReturnType, out var taskResultType))
        {
            if (taskResultType is null)
            {
                source.AppendLine("return global::System.Threading.Tasks.Task.CompletedTask;");
            }
            else if (StubDefaults.RequiresNotImplemented(taskResultType))
            {
                source.AppendLine("throw new global::System.NotImplementedException();");
            }
            else
            {
                source.Append("return global::System.Threading.Tasks.Task.FromResult<")
                    .Append(taskResultType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                    .Append(">(")
                    .Append(StubDefaults.GetReturnExpression(taskResultType))
                    .AppendLine(");");
            }
        }
        else if (!method.ReturnsVoid)
        {
            if (StubDefaults.RequiresNotImplemented(method.ReturnType))
            {
                source.AppendLine("throw new global::System.NotImplementedException();");
            }
            else
            {
                source.Append("return ").Append(StubDefaults.GetReturnExpression(method.ReturnType)).AppendLine(";");
            }
        }

        source.Pop().AppendLine("}");
    }

    public static string GetMethodDeclaration(this StubDefinition type, IMethodSymbol method)
    {
        var returnType = method.ReturnType.ToQualifiedString();
        var parameters = string.Join(", ", method.Parameters.Select(parameter =>
            (parameter.IsParams ? "params " : string.Empty) +
            (parameter.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty
            }) + parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " " + parameter.Name));
        var typeParameters = method.Arity == 0 ? string.Empty : "<" + string.Join(", ", method.TypeParameters.Select(parameter => parameter.Name)) + ">";
        var overrideModifier = method.ContainingType.TypeKind == TypeKind.Interface ? string.Empty : "override ";
        var accessibility = method.GetMemberAccessibility(type.ContainingAssembly);

        return $"{accessibility} {overrideModifier}{returnType} {method.Name}{typeParameters}({parameters})";
    }
}