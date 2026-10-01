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
}
