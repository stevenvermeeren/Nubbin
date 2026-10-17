using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class EventEmitterTests
{
    private static MissingEvent Missing(IEventSymbol @event, StubDefinition type) => new(@event, type);

    [Fact]
    public void AppendEventGeneratesPublicInterfaceEventDeclaration()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { event System.EventHandler Changed; }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var @event = type.LeafType!.GetMembers().OfType<IEventSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendEvent(Missing(@event, type));

        var result = builder.ToString();

        Assert.Contains("public event global::System.EventHandler Changed", result);
        Assert.Contains("{ add { } remove { } }", result);
    }
}
