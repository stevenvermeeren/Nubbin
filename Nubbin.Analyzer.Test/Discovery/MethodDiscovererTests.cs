using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Test.Generator.Discovery;

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

        var methods = _target.GetSymbolsMissingImplementation(StubDefinition.FromINamedTypeSymbol(type)).OfType<MissingMethod>().ToArray();

        foreach (var method in methods)
        {
            method.Process(methods);
        }

        var remainingMethods = methods.Where(method => method.ShouldEmit).ToArray();
        Assert.Single(remainingMethods);
        Assert.Equal("BaseMethod", remainingMethods[0].Method.Name);
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

        var methods = _target.GetSymbolsMissingImplementation(StubDefinition.FromINamedTypeSymbol(type)).OfType<MissingMethod>().ToArray();

        foreach (var method in methods)
        {
            method.Process(methods);
        }

        var remainingMethods = methods.Where(method => method.ShouldEmit).Select(method => method.Method.Name).ToArray();
        Assert.Equal(["BaseMethod", "InterfaceMethod"], remainingMethods);
    }
}