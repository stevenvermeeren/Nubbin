using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Analyzer.Discovery;

internal class MissingEvent : MissingMember
{
    public IEventSymbol Event { get; }

    public MissingEvent(IEventSymbol @event, StubDefinition context) : base(@event, context)
    {
        Event = @event;
    }

    public override void Output(IndentedStringBuilder builder)
    {
        builder.AppendEvent(this);
    }

    public override bool Implements(MissingMember other)
    {
        if (other is not MissingEvent evt)
            return false;

        return evt.Event.Name == Event.Name
            && SymbolEqualityComparer.Default.Equals(evt.Event.Type, Event.Type);
    }

    public override bool ConflictsWith(MissingMember other)
    {
        return other.ConflictsWith(this);
    }

    public override bool ConflictsWith(MissingProperty other)
    {
        return other.Property.Name == Event.Name;
    }

    public override bool ConflictsWith(MissingEvent other)
    {
        return other.Event.Name == Event.Name
            && !SymbolEqualityComparer.Default.Equals(other.Event.Type, Event.Type);
    }

    public override bool ConflictsWith(MissingMethod other)
    {
        return other.Method.Name == Event.Name;
    }
}
