using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Emitting;

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

        Assert.Contains("internal sealed class SubjectPropertyContainer", result);
        Assert.Contains("public string? Name { get; set; } = default;", result);
    }

    [Fact]
    public void AppendPropertyStorageLookup_GeneratesLookup()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string? Name { get; set; } }"
        );

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));

        var builder = new IndentedStringBuilder();
        builder.AppendPropertyStorageLookup(type);

        var result = builder.ToString();

        Assert.Contains("GetPropertyHelper(this global::Example.Subject owner)", result);
    }

    [Fact]
    public void AppendPropertyStorageLookup_GeneratesLookupForGenericType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject<T> { public string? Name { get; set; } }"
        );

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject`1"));

        var builder = new IndentedStringBuilder();
        builder.AppendPropertyStorageLookup(type);

        var result = builder.ToString();

        Assert.Contains("GetPropertyHelper<T>(this global::Example.Subject<T> owner)", result);
    }
}
