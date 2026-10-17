using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Analyzer.Discovery;

internal class MissingMethod : MissingMember
{
    public IMethodSymbol Method { get; }

    public MissingMethod(IMethodSymbol method, StubDefinition context) : base(method, context)
    {
        Method = method;
    }

    public override void Output(IndentedStringBuilder builder)
    {
        builder.AppendMethod(this);
    }

    public override bool Implements(MissingMember other)
    {
        if (other is not MissingMethod method)
            return false;

        return method.Method.Name == Method.Name
            && method.Method.Parameters.Select(p => p.Type).SequenceEqual(Method.Parameters.Select(p => p.Type), SymbolEqualityComparer.Default)
            && SymbolEqualityComparer.Default.Equals(method.Method.ReturnType, Method.ReturnType)
            && method.Method.TypeParameters.Length == Method.TypeParameters.Length
            && !TypeParametersIncompatible(method.Method, Method);
    }

    public override bool ConflictsWith(MissingMember other)
    {
        return other.ConflictsWith(this);
    }

    public override bool ConflictsWith(MissingProperty other)
    {
        return other.Symbol.Name == Method.Name;
    }

    public override bool ConflictsWith(MissingEvent other)
    {
        return other.Symbol.Name == Method.Name;
    }

    public override bool ConflictsWith(MissingMethod other)
    {
        return other.Method.Name == Method.Name
            && (
                !SymbolEqualityComparer.Default.Equals(other.Method.ReturnType, Method.ReturnType) ||
                TypeParametersIncompatible(other.Method, Method)
            );
    }

    private static bool TypeParametersIncompatible(IMethodSymbol method1, IMethodSymbol method2)
    {
        if (method1.TypeParameters.Length != method2.TypeParameters.Length)
            return false;
        
        for (int i = 0; i < method1.TypeParameters.Length; i++)
        {
            if (method1.TypeParameters[i].HasConstructorConstraint != method2.TypeParameters[i].HasConstructorConstraint)
                return true;
            if (method1.TypeParameters[i].HasNotNullConstraint != method2.TypeParameters[i].HasNotNullConstraint)
                return true;
            if (method1.TypeParameters[i].HasReferenceTypeConstraint != method2.TypeParameters[i].HasReferenceTypeConstraint)
                return true;
            if (method1.TypeParameters[i].HasValueTypeConstraint != method2.TypeParameters[i].HasValueTypeConstraint)
                return true;
            if (!method1.TypeParameters[i].ConstraintTypes.SequenceEqual(method2.TypeParameters[i].ConstraintTypes))
                return true;
        }
        return false;
    }
}
