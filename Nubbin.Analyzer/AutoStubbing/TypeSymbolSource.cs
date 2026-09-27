using System.Text;
using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

internal class TypeSymbolSource(INamedTypeSymbol symbol) : AutoStubSource(symbol)
{
    public override string GetInstantiationExpression()
    {
        var builder = new StringBuilder();
        builder
            .Append("new global::Nubbin.Generated.")
            .Append(Symbol.GetStubTypeNameWithNamespace())
            .Append("()");
        return builder.ToString();
    }
}
