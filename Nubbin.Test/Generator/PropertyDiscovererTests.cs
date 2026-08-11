using Microsoft.CodeAnalysis;

namespace Nubbin.Test.Generator;

public class PropertyDiscovererTests
{
    private readonly PropertyDiscoverer _target = new();

    [Fact]
    public void FindsMissingMembersAcrossBaseTypesAndInterfacesWithoutDuplicates()
    {
        const string source = """
            public abstract class Base
            {
                public abstract int Value { get; set; }
            }
            public interface IContract
            {
                int Value { get; set; }
            }
            public class Subject : Base, IContract
            {
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        var properties = _target.GetSymbolsMissingImplementation(type).ToArray();
        Assert.Single(properties);
        Assert.Equal("Value", properties[0].Name);
    }

    [Fact]
    public void FindsAbstractPropertiesThatRemainUnimplemented()
    {
        const string source = """
            public abstract class Base
            {
                public abstract string Name { get; }
                public abstract int Age { get; }
            }
            public interface IContract
            {
                bool Exists { get; set; }
            }
            public class Subject : Base, IContract
            {
                public override int Age { get; }
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        var properties = _target.GetSymbolsMissingImplementation(type).ToArray();

        var propertyNames = properties.Select(p => p.Name);
        Assert.Equal(["Name", "Exists"], propertyNames);
    }
}