using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Analyzer.Emitting;

internal static class EventEmitter
{
    public static void AppendEvent(
        this IndentedStringBuilder source,
        MissingEvent @event)
    {
        source
            .Append(GetEventDeclaration(@event))
            .AppendLine(" { add { } remove { } }");
    }

    private static string GetEventDeclaration(MissingEvent @event)
    {
        var eventType = @event.Event.Type.ToQualifiedString();
        var overrideModifier = @event.GetOverrideModifier();
        var accessibility = @event.FormatMemberAccessibility();
        var name = @event.FormatName();

        return $"{accessibility}{overrideModifier}event {eventType} {name}";
    }
}