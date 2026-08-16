using Microsoft.CodeAnalysis;
using Nubbin.Generator;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class EventEmitterTests
{
    [Fact]
    public void AppendEventGeneratesPublicInterfaceEventDeclaration()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { event System.EventHandler Changed; }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var @event = type.LeafType!.GetMembers().OfType<IEventSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendEvent(type, @event);

        var result = builder.ToString();

        Assert.Contains("public event global::System.EventHandler Changed", result);
        Assert.Contains("{ add { } remove { } }", result);
    }
}
