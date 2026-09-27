using System.Text;
using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

internal class DelegateSource(INamedTypeSymbol symbol) : AutoStubSource(symbol)
{
    public override string GetInstantiationExpression()
    {
        var builder = new StringBuilder();
        builder
            .Append("new ")
            .Append(Symbol.GetFullyQualifiedName())
            .Append("(")
            .Append(Symbol.GetDelegateExpression())
            .Append(")");
        return builder.ToString();
    }
}
