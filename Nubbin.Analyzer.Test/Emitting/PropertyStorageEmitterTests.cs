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
    public void AppendPropertyStorage_GeneratesGetterAndSetterForSetterOnlyNonConstructibleProperty()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public class ValueTypeHolder(int value) { }
            public abstract class Subject { public abstract ValueTypeHolder Value { set; } }
            """
        );

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var properties = type.LeafType!.GetMembers().OfType<IPropertySymbol>().ToArray();

        var builder = new IndentedStringBuilder();
        builder.AppendPropertyStorage(type, properties);

        var result = builder.ToString();

        Assert.Contains("public global::Example.ValueTypeHolder Value", result);
        Assert.Contains("get => __Nubbin__Value", result);
        Assert.Contains("set => __Nubbin__Value = value;", result);
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

        Assert.Contains("extension(global::Example.Subject owner)", result);
        Assert.Contains("internal global::Example.Subject.SubjectPropertyContainer Properties", result);
        Assert.Contains("=> owner.__Nubbin__PropertyContainer;", result);
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

        Assert.Contains("extension<T>(global::Example.Subject<T> owner)", result);
        Assert.Contains("owner.__Nubbin__PropertyContainer", result);
        Assert.DoesNotContain("(owner as global::Example.Subject<T>)", result);
    }

    [Fact]
    public void AppendPropertyStorageLookup_GeneratesLookupForNestedType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Outer { public class Middle { public class Subject { public string? Name { get; set; } } } }"
        );

        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Outer+Middle+Subject"));

        var builder = new IndentedStringBuilder();
        builder.AppendPropertyStorageLookup(type);

        var result = builder.ToString();

        Assert.Contains("extension(global::Example.Outer.Middle.Subject owner)", result);
        Assert.Contains("owner.__Nubbin__PropertyContainer", result);
    }

    [Fact]
    public void AppendPropertyStorageLookup_UsesCastWhenTargetTypeDiffersFromContainerType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string? Name { get; set; } }"
        );

        var type = new StubDefinition
        {
            Name = "SubjectStub",
            Namespace = "Example",
            ContainingAssembly = compilation.Assembly,
            LeafType = GeneratorTestHelpers.GetType(compilation, "Example.Subject"),
            BaseType = GeneratorTestHelpers.GetType(compilation, "Example.Subject")
        };

        var builder = new IndentedStringBuilder();
        builder.AppendPropertyStorageLookup(type);

        var result = builder.ToString();

        Assert.Contains("(owner as global::Example.SubjectStub)?.__Nubbin__PropertyContainer", result);
    }
}
