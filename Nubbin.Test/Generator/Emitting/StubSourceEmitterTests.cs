using Nubbin.Generator;
using Nubbin.Generator.Emitting;

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
}
