using Nubbin.Analyzer;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class StubSourceEmitterTests
{
    [Fact]
    public void EmitGeneratesNamespaceAndStubClassForInterface()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { int Value { get; set; } } public class IComponentStub : IComponent;");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponentStub"));

        var result = StubSourceEmitter.Emit(type);

        Assert.Contains("namespace Example", result);
        Assert.Contains("class IComponentStub", result);
        Assert.Contains("public int Value", result);
    }

    [Fact]
    public void EmitGeneratesNestedStubWithPropertyStorageLookup()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;

            public abstract class Outer
            {
                public abstract class Middle
                {
                    public abstract int Value { get; }
                }

                public class MiddleStub : Middle { }
            }
            """);

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Outer+MiddleStub"));

        var result = StubSourceEmitter.Emit(type);

        Assert.Contains("class Outer", result);
        Assert.Contains("class MiddleStub", result);
        Assert.Contains("internal readonly MiddleStubPropertyContainer __Nubbin__PropertyContainer = new();", result);
        Assert.Contains("GetPropertyHelper(this global::Example.Outer.MiddleStub owner)", result);
        Assert.Contains("get => __Nubbin__PropertyContainer.Value;", result);
    }

    [Fact]
    public void EmitsExplicitImplementations()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            using System.Collections;
            using System.Collections.Generic;
            namespace Example;

            public interface IDoubleEnumerable : IEnumerable, IEnumerable<string>
            """);

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IDoubleEnumerable"));

        var result = StubSourceEmitter.Emit(type);

        Assert.Contains("public global::System.Collections.Generic.IEnumerator<string> GetEnumerator()", result);
        Assert.Contains("global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()", result);
    }

    [Fact]
    public void EmitsExplicitEvent()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;

            public abstract class Base { public abstract string Name(); }
            public interface Interface { event System.EventHandler Name; }
            public class Target : Base, Interface;
            """);

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Target"));

        var result = StubSourceEmitter.Emit(type);

        Assert.Contains("event global::System.EventHandler global::Example.Interface.Name", result);
    }
}
