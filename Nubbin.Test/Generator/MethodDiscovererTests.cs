using Microsoft.CodeAnalysis;

namespace Nubbin.Test.Generator;

public class MethodDiscovererTests
{
    private readonly MethodDiscoverer _target = new();

    [Fact]
    public void FindsMissingMembersAcrossBaseTypesAndInterfacesWithoutDuplicates()
    {
        const string source = """
            public abstract class Base
            {
                public abstract string BaseMethod();
            }
            public interface IContract
            {
                string BaseMethod();
                string InterfaceMethod();
            }
            public class Subject : Base, IContract
            {
                public string InterfaceMethod() => \"implemented\";
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        var methods = _target.GetSymbolsMissingImplementation(type).ToArray();
        Assert.Single(methods);
        Assert.Equal("BaseMethod", methods[0].Name);
    }

    [Fact]
    public void FindsAbstractMethodsThatRemainUnimplemented()
    {
        const string source = """
            public abstract class Base
            {
                public abstract string BaseMethod();
                protected abstract int Create(int value);
            }
            public interface IContract
            {
                string InterfaceMethod();
            }
            public class Subject : Base, IContract
            {
                protected override int Create(int value) => value;
            }
            """;
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");

        var methods = _target.GetSymbolsMissingImplementation(type).ToArray();

        var methodNames = methods.Select(p => p.Name);
        Assert.Equal(["BaseMethod", "InterfaceMethod"], methodNames);
    }
}