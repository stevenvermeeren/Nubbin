using Microsoft.CodeAnalysis;
using Nubbin.Generator;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class PropertyEmitterTests
{
    [Fact]
    public void AppendPropertyUsesBackingStorageWhenRequired()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string Name { get; } }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(type, property, useStorage: true);

        var result = builder.ToString();

        Assert.Contains("public override string Name", result);
        Assert.Contains("get => global::Nubbin.Stubs.GetPropertyHelper(this).Name;", result);
    }

    [Fact]
    public void AppendAutoPropertyBodyGeneratesAutomaticPropertyForValueType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public int Count { get; set; } }");
        var property = GeneratorTestHelpers.GetType(compilation, "Example.Subject").GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendAutoPropertyBody(property);

        var result = builder.ToString();

        Assert.Contains("{ get; set; } = default;", result);
    }

    [Fact]
    public void AppendProperty_UseStorage_WritesSetterOnly()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string Name { set; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(type, property, useStorage: true);

        var result = builder.ToString();

        Assert.Contains("set => global::Nubbin.Stubs.GetPropertyHelper(this).Name = value;", result);
        Assert.DoesNotContain("get => global::Nubbin.Stubs.GetPropertyHelper(this).Name;", result);
    }

    [Fact]
    public void AppendProperty_UseStorage_WritesGetterAndSetter()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string Name { get; set; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(type, property, useStorage: true);

        var result = builder.ToString();

        Assert.Contains("get => global::Nubbin.Stubs.GetPropertyHelper(this).Name;", result);
        Assert.Contains("set => global::Nubbin.Stubs.GetPropertyHelper(this).Name = value;", result);
    }

    [Fact]
    public void AppendAutoPropertyBody_GeneratesNotImplementedForNonConstructibleReferenceType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string Name { get; set; } }"
        );
        var property = GeneratorTestHelpers.GetType(compilation, "Example.Subject").GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendAutoPropertyBody(property);

        var result = builder.ToString();

        Assert.Contains("get => throw new global::System.NotImplementedException();", result);
    }
}
