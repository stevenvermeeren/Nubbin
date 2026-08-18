using Microsoft.CodeAnalysis;

namespace Nubbin.Generator.Emitting;

internal static class EventEmitter
{
    public static void AppendEvent(
        this IndentedStringBuilder source,
        StubDefinition type,
        IEventSymbol _event)
    {
        source
            .Append(type.GetEventDeclaration(_event))
            .AppendLine(" { add { } remove { } }");
    }

    private static string GetEventDeclaration(this StubDefinition type, IEventSymbol _event)
    {
        var eventType = _event.Type.ToQualifiedString();
        var overrideModifier = _event.ContainingType.TypeKind == TypeKind.Interface
            ? string.Empty
            : "override ";
        var accessibility = _event.GetMemberAccessibility(type.ContainingAssembly);

        return $"{accessibility} {overrideModifier}event {eventType} {_event.Name}";
    }
}