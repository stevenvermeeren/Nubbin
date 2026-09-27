using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

internal abstract class AutoStubSource
{
    public INamedTypeSymbol Symbol { get; }

    public AutoStubSource(INamedTypeSymbol symbol)
    {
        Symbol = symbol;
    }

    public string GetFullyQualifiedNameWithTypeParams() => Symbol.GetFullyQualifiedNameWithTypeParams();

    public abstract string GetInstantiationExpression();
}
