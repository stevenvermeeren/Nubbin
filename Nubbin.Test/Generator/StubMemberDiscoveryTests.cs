using Microsoft.CodeAnalysis;

namespace Nubbin.Test.Generator;

public class StubMemberDiscoveryTests
{
    [Fact]
    public void FindsMissingMembersAcrossBaseTypesAndInterfacesWithoutDuplicates()
    {
        const string source = """
            public abstract class Base
            {
                public abstract string BaseMethod();
                public abstract int Value { get; set; }
            }
            public interface IContract
            {
                string InterfaceMethod();
                int Value { get; set; }
            }
            public class Subject : Base, IContract
            {
                public override string BaseMethod() => \"implemented\";
                public string InterfaceMethod() => \"implemented\";
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        Assert.Empty(Nubbin.Generator.StubMemberDiscovery.FindMissingMethods(type));
        var properties = Nubbin.Generator.StubMemberDiscovery.FindMissingProperties(type).ToArray();
        Assert.Single(properties);
        Assert.Equal("Value", properties[0].Name);
    }

    [Fact]
    public void FindsAbstractMethodsAndPropertiesThatRemainUnimplemented()
    {
        const string source = """
            public abstract class Base
            {
                protected abstract int Create(int value);
                public abstract string Name { get; }
            }
            public class Subject : Base
            {
                protected override int Create(int value) => value;
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        var methods = Nubbin.Generator.StubMemberDiscovery.FindMissingMethods(type).ToArray();
        var properties = Nubbin.Generator.StubMemberDiscovery.FindMissingProperties(type).ToArray();

        Assert.Empty(methods);
        Assert.Equal("Name", Assert.Single(properties).Name);
    }
}