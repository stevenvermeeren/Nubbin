using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Test.Generator.Discovery;

public class EventDiscovererTests
{
    private readonly EventDiscoverer _target = new();

    [Fact]
    public void FindsMissingMembersAcrossBaseTypesAndInterfacesWithoutDuplicates()
    {
        const string source = """
            public abstract class Base
            {
                public abstract event EventHandler BaseEvent;
            }
            public interface IContract
            {
                event EventHandler BaseEvent;
            }
            public class Subject : Base, IContract
            {
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        var events = _target.GetSymbolsMissingImplementation(StubDefinition.FromINamedTypeSymbol(type)).OfType<MissingEvent>().ToArray();

        foreach (var @event in events)
        {
            @event.Process(events);
        }

        var remainingEvents = events.Where(@event => @event.ShouldEmit).ToArray();
        Assert.Single(remainingEvents);
        Assert.Equal("BaseEvent", remainingEvents[0].Event.Name);
    }

    [Fact]
    public void FindsAbstractPropertiesThatRemainUnimplemented()
    {
        const string source = """
            public abstract class Base
            {
                public abstract event EventHandler BaseEvent;
            }
            public interface IContract
            {
                event EventHandler InterfaceEvent1;
                event EventHandler InterfaceEvent2;
            }
            public class Subject : Base, IContract
            {
                public event EventHandler InterfaceEvent1 { add { } remove { } }
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        var events = _target.GetSymbolsMissingImplementation(StubDefinition.FromINamedTypeSymbol(type)).OfType<MissingEvent>().ToArray();

        foreach (var @event in events)
        {
            @event.Process(events);
        }

        var eventNames = events.Where(@event => @event.ShouldEmit).Select(e => e.Event.Name).ToArray();
        Assert.Equal(["BaseEvent", "InterfaceEvent2"], eventNames);
    }
}