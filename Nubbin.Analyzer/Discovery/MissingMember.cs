using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Analyzer.Discovery;

internal abstract class MissingMember
{
    public ISymbol Symbol { get; }
    public StubDefinition Context { get; }
    public bool ImplementExplicitly { get; private set; }
    public bool ShouldEmit { get; private set; } = true;

    public MissingMember(ISymbol symbol, StubDefinition context)
    {
        Symbol = symbol;
        Context = context;
    }

    public abstract void Output(IndentedStringBuilder builder);

    public abstract bool ConflictsWith(MissingMember other);
    public abstract bool ConflictsWith(MissingProperty other);
    public abstract bool ConflictsWith(MissingEvent other);
    public abstract bool ConflictsWith(MissingMethod other);

    public abstract bool Implements(MissingMember other);

    public void Process(IEnumerable<MissingMember> allMembers)
    {
        if (Symbol.ContainingType.TypeKind != TypeKind.Interface)
            return;

        foreach (var member in allMembers.Except([this]))
        {
            if (member.ImplementExplicitly || !member.ShouldEmit)
                continue;

            if (member.ConflictsWith(this))
            {
                this.ImplementExplicitly = true;
                return;
            }
            if (member.Implements(this))
            {
                this.ShouldEmit = false;
                return;
            }
        }
    }
}
