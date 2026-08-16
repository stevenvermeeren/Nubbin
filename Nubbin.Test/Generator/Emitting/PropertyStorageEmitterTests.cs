using Microsoft.CodeAnalysis;
using Nubbin.Generator;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class PropertyStorageEmitterTests
{
    [Fact]
    public void AppendPropertyStorage_GeneratesHelperAndStorage()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string? Name { get; set; } }"
        );

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var properties = type.LeafType!.GetMembers().OfType<IPropertySymbol>().ToArray();

        var builder = new IndentedStringBuilder();
        builder.AppendPropertyStorage(type, properties);

        var result = builder.ToString();

        Assert.Contains("GetPropertyHelper(this global::Example.Subject owner)", result);
        Assert.Contains("namespace Example.Nubbin", result);
        Assert.Contains("public string? Name", result);
        Assert.Contains("{ get; set; } = default;", result);
    }
}
