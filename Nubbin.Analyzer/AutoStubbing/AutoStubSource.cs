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

    public override bool Equals(object obj)
    {
        if (obj is AutoStubSource other)
            return SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol);
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return SymbolEqualityComparer.Default.GetHashCode(Symbol);
    }
}
