using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;
using Nubbin.Test.Generator;

namespace Nubbin.Test.Generator.Discovery;

public class MissingEventTests
{
    [Fact]
    public void MissingEvent_RecognizesSameSignatureAndConflictsWithOtherKinds()
    {
        const string source = """
            public interface IEventContract
            {
                event System.EventHandler Value;
            }

            public interface IPropertyContract
            {
                int Value { get; }
            }

            public interface IMethodContract
            {
                int Value();
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IEventContract"));
        var @event = new MissingEvent(GeneratorTestHelpers.GetType(compilation, "IEventContract").GetMembers("Value").OfType<IEventSymbol>().Single(), type);
        var sameEvent = new MissingEvent(GeneratorTestHelpers.GetType(compilation, "IEventContract").GetMembers("Value").OfType<IEventSymbol>().Single(), type);
        var property = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IPropertyContract").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);
        var method = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IMethodContract").GetMembers("Value").OfType<IMethodSymbol>().Single(), type);

        Assert.True(@event.Implements(sameEvent));
        Assert.True(@event.ConflictsWith(property));
        Assert.True(@event.ConflictsWith(method));
    }
}
