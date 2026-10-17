using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Test.Generator.Discovery;

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

        var properties = _target.GetSymbolsMissingImplementation(StubDefinition.FromINamedTypeSymbol(type)).OfType<MissingProperty>().ToArray();

        foreach (var property in properties)
        {
            property.Process(properties);
        }

        var remainingProperties = properties.Where(property => property.ShouldEmit).ToArray();
        Assert.Single(remainingProperties);
        Assert.Equal("Value", remainingProperties[0].Property.Name);
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

        var properties = _target.GetSymbolsMissingImplementation(StubDefinition.FromINamedTypeSymbol(type)).OfType<MissingProperty>().ToArray();

        foreach (var property in properties)
        {
            property.Process(properties);
        }

        var propertyNames = properties.Where(property => property.ShouldEmit).Select(p => p.Property.Name).ToArray();
        Assert.Equal(["Name", "Exists"], propertyNames);
    }
}