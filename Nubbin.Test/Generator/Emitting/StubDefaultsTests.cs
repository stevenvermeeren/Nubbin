using Microsoft.CodeAnalysis;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class StubDefaultsTests
{
    [Theory]
    [InlineData("System.Threading.Tasks.Task", true, false)]
    [InlineData("System.Threading.Tasks.Task<string>", true, true)]
    [InlineData("string", false, false)]
    public void IsTaskIdentifiesTaskShapes(string typeName, bool expectedIsTask, bool expectedHasResult)
    {
        var compilation = GeneratorTestHelpers.CreateCompilation($"class Subject {{ {typeName} Value => default!; }}");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.Equal(expectedIsTask, StubDefaults.IsTask(returnType, out var resultType));
        Assert.Equal(expectedHasResult, resultType is not null);
    }

    [Theory]
    [InlineData("System.Collections.Generic.IEnumerable<string>", "global::System.Array.Empty<string>()")]
    [InlineData("System.Collections.Generic.ICollection<int>", "new global::System.Collections.Generic.List<int>()")]
    [InlineData("System.Collections.Generic.IDictionary<string, int>", "new global::System.Collections.Generic.Dictionary<string, int>()")]
    [InlineData("int", "default")]
    public void GetReturnExpressionUsesCompatibleDefaults(string typeName, string expectedExpression)
    {
        var compilation = GeneratorTestHelpers.CreateCompilation($"class Subject {{ {typeName} Value => default!; }}");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.Equal(expectedExpression, StubDefaults.GetReturnExpression(returnType));
    }

    [Fact]
    public void RequiresNotImplementedForNonNullableUnsupportedReference()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation("class Subject { string Value => \"value\"; }");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.True(StubDefaults.RequiresNotImplemented(returnType));
    }

    [Fact]
    public void ConstructibleNonNullableReferenceGetsNewInstance()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation("class Value { } class Subject { Value Property => new(); }");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Property").OfType<IPropertySymbol>().Single().Type;

        Assert.Equal("new global::Value()", StubDefaults.GetReturnExpression(returnType));
        Assert.False(StubDefaults.RequiresNotImplemented(returnType));
    }
}