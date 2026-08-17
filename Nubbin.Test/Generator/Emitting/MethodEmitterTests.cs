using Microsoft.CodeAnalysis;
using Nubbin.Generator;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class MethodEmitterTests
{
    [Fact]
    public void AppendMethodGeneratesMethodDeclarationAndReturnBody()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { string? Name(string value); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("Name").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("public string? Name(string value)", result);
        Assert.Contains("return default;", result);
    }

    [Fact]
    public void AppendMethod_ObliviousType_BecomesNullable()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { string Name(string value); }",
            NullableContextOptions.Disable);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("Name").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("public string? Name(string? value)", result);
        Assert.Contains("return default;", result);
    }

    [Fact]
    public void AppendMethod_OutParameter_ThrowsWhenNotConstructible()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { void TryGet(out string value); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("TryGet").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("throw new global::System.NotImplementedException();", result);
    }

    [Fact]
    public void AppendMethod_OutParameter_AssignsConstructible()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class MyType { public MyType() {} } public interface IComponent { void TryGet(out MyType value); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("TryGet").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("value = new global::Example.MyType()", result);
    }

    [Fact]
    public void AppendMethod_Task_ReturnsCompletedTask()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { System.Threading.Tasks.Task DoAsync(); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("DoAsync").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("global::System.Threading.Tasks.Task.CompletedTask", result);
    }

    [Fact]
    public void AppendMethod_TaskOfT_ReturnsTaskForNullable()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { System.Threading.Tasks.Task<string?> GetAsync(); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("GetAsync").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("public global::System.Threading.Tasks.Task<string?> GetAsync()", result);
        Assert.Contains("global::System.Threading.Tasks.Task.FromResult<string?>(default)", result);
    }

    [Fact]
    public void AppendMethod_TaskOfT_ReturnsTaskForOblivious()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { System.Threading.Tasks.Task<string> GetAsync(); }",
            NullableContextOptions.Disable);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("GetAsync").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("public global::System.Threading.Tasks.Task<string?>? GetAsync()", result);
        Assert.Contains("global::System.Threading.Tasks.Task.FromResult<string?>(default)", result);
    }

    [Fact]
    public void AppendMethod_TaskOfT_ThrowsForUnconstructible()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { System.Threading.Tasks.Task<string> GetAsync(); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("GetAsync").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("public global::System.Threading.Tasks.Task<string> GetAsync()", result);
        Assert.Contains("throw new global::System.NotImplementedException();", result);
    }

    [Fact]
    public void AppendMethod_TaskOfT_FromResult_WithConstructible()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class MyType { public MyType() {} } public interface IComponent { System.Threading.Tasks.Task<MyType> GetAsync(); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("GetAsync").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("Task.FromResult<global::Example.MyType>", result);
        Assert.Contains("new global::Example.MyType()", result);
    }

    [Fact]
    public void AppendMethod_ReturnsConstructibleInstance()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class MyType { public MyType() {} } public interface IComponent { MyType Create(); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("Create").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("return new global::Example.MyType()", result);
    }

    [Fact]
    public void AppendMethod_IncludesOverrideModifierForClassMember()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public int Compute() => 1; } ");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var method = type.LeafType!.GetMembers("Compute").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("override", result);
    }

    [Fact]
    public void AppendMethod_Declaration_IncludesParamsRefInModifiers()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { void M(ref int a, in int b, params int[] rest); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("M").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("ref ", result);
        Assert.Contains("in ", result);
        Assert.Contains("params ", result);
    }

    [Fact]
    public void AppendMethod_Declaration_IncludesGenericTypeParameters()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { T Echo<T>(T value); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("Echo").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("<T>", result);
    }

    [Fact]
    public void AppendMethod_VoidMethod_DoesNotContainReturn()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { void Do(); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("Do").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.DoesNotContain("return ", result);
    }

    [Fact]
    public void AppendMethod_OutParameter_Task_AssignsNewTask()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { void TryGet(out System.Threading.Tasks.Task value); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("TryGet").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("value = global::System.Threading.Tasks.Task.CompletedTask", result);
    }

    [Fact]
    public void AppendMethod_OutParameter_TaskOfT_ThrowsWhenUnconstructible()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface IComponent { void TryGet(out System.Threading.Tasks.Task<string> value); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("TryGet").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("throw new global::System.NotImplementedException();", result);
    }

    [Fact]
    public void AppendMethod_OutParameter_TaskOfT_ReturnsResultWhenTIsConstructible()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class MyType { public MyType() {} } public interface IComponent { void TryGet(out System.Threading.Tasks.Task<MyType> value); }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IComponent"));
        var method = type.LeafType!.GetMembers("TryGet").OfType<IMethodSymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendMethod(method, type);

        var result = builder.ToString();

        Assert.Contains("value = global::System.Threading.Tasks.Task.FromResult<global::Example.MyType>(new global::Example.MyType())", result);
    }
}
