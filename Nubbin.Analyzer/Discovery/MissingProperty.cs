using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Analyzer.Discovery;

internal class MissingProperty : MissingMember
{
    public IPropertySymbol Property { get; }
    public bool RequiresPropertyStorage
        => !ImplementExplicitly && (Property.GetMethod is null) != (Property.SetMethod is null);

    public MissingProperty(IPropertySymbol property, StubDefinition context) : base(property, context)
    {
        Property = property;
    }

    public override void Output(IndentedStringBuilder builder)
    {
        builder.AppendProperty(this);
    }

    public override bool Implements(MissingMember other)
    {
        if (other is not MissingProperty prop)
            return false;

        return prop.Property.Name == Property.Name
            && (
                SymbolEqualityComparer.Default.Equals(prop.Property.Type, Property.Type) &&
                (prop.Property.GetMethod is null || Property.GetMethod is not null) &&
                (prop.Property.SetMethod is null || Property.SetMethod is not null)
            );
    }

    public override bool ConflictsWith(MissingMember other)
    {
        return other.ConflictsWith(this);
    }

    public override bool ConflictsWith(MissingProperty other)
    {
        return other.Property.Name == Property.Name
            && (
                !SymbolEqualityComparer.Default.Equals(other.Property.Type, Property.Type) ||
                other.Property.GetMethod is null != Property.GetMethod is null ||
                other.Property.SetMethod is null != Property.SetMethod is null
            );
    }

    public override bool ConflictsWith(MissingEvent other)
    {
        return other.Event.Name == Property.Name;
    }

    public override bool ConflictsWith(MissingMethod other)
    {
        return other.Method.Name == Property.Name;
    }
}
