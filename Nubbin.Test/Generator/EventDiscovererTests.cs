using Microsoft.CodeAnalysis;

namespace Nubbin.Test.Generator;

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

        var events = _target.GetSymbolsMissingImplementation(type).ToArray();
        Assert.Single(events);
        Assert.Equal("BaseEvent", events[0].Name);
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

        var events = _target.GetSymbolsMissingImplementation(type).ToArray();

        var eventNames = events.Select(e => e.Name);
        Assert.Equal(["BaseEvent", "InterfaceEvent2"], eventNames);
    }
}