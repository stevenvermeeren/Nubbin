using Microsoft.CodeAnalysis;
using Nubbin.Generator;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class ClassEmitterTests
{
    [Fact]
    public void WithClassGeneratesStaticPartialClassDeclaration()
    {
        var builder = new IndentedStringBuilder();

        builder.WithClass(
            new ClassEmitter.Definition(Accessibility.Private, "Stub") { IsStatic = true, IsPartial = true },
            () => builder.AppendLine("public static T Auto<T>() => default!;"));

        var result = builder.ToString();

        Assert.Contains("private static partial class Stub", result);
        Assert.Contains("public static T Auto<T>() => default!;", result);
    }

    [Fact]
    public void WithClassIncludesInterfaceBaseTypesWhenProvided()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation("public interface IComponent { int Value { get; set; } }");
        var baseType = compilation.GetTypeByMetadataName("IComponent")!;
        var builder = new IndentedStringBuilder();

        builder.WithClass(
            new ClassEmitter.Definition(Accessibility.Public, "Container") { BaseTypes = [baseType] },
            () => builder.AppendLine("public void Example() { }"));

        var result = builder.ToString();

        Assert.Contains("public class Container : global::IComponent", result);
    }
}
