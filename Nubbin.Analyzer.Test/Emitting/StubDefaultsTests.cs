using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

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
    [InlineData("System.Collections.Generic.IReadOnlyCollection<int>", "global::System.Array.Empty<int>()")]
    [InlineData("System.Collections.Generic.IReadOnlyList<int>", "global::System.Array.Empty<int>()")]
    [InlineData("System.Collections.Generic.ICollection<int>", "new global::System.Collections.Generic.List<int>()")]
    [InlineData("System.Collections.Generic.IList<int>", "new global::System.Collections.Generic.List<int>()")]
    [InlineData("System.Collections.Generic.ISet<int>", "new global::System.Collections.Generic.HashSet<int>()")]
    [InlineData("System.Collections.Generic.IDictionary<string, int>", "new global::System.Collections.Generic.Dictionary<string, int>()")]
    [InlineData("System.Collections.Generic.IReadOnlyDictionary<string, int>", "new global::System.Collections.Generic.Dictionary<string, int>()")]
    [InlineData("System.Collections.IEnumerable", "global::System.Array.Empty<object>()")]
    [InlineData("System.Collections.ICollection", "new global::System.Collections.ArrayList()")]
    [InlineData("System.Collections.IList", "new global::System.Collections.ArrayList()")]
    [InlineData("System.Action", "() => { }")]
    [InlineData("delegate bool Test(); Test", "() => default")]
    [InlineData("int", "default")]
    public void GetReturnExpressionUsesCompatibleDefaults(string typeName, string expectedExpression)
    {
        var compilation = GeneratorTestHelpers.CreateCompilation($"class Subject {{ {typeName} Value => default!; }}");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.Equal(expectedExpression, StubDefaults.GetReturnExpression(returnType));
    }

    [Fact]
    public void GetReturnExpression_UsesConstructorConstraintForTypeParameters()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "class Subject<T> where T : new() { public T Value => default!; }");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject`1").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.Equal("new T()", StubDefaults.GetReturnExpression(returnType));
    }

    [Fact]
    public void CanBeInstantiated_HandlesTypeParametersAndNullableAnnotations()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            class StructSubject<T> where T : struct
            {
                public T Value => default;
            }

            class NullableReferenceSubject<T> where T : class
            {
                public T? Value => default;
            }

            class ReferenceSubject<T> where T : class
            {
                public T Value => default!;
            }
            """);

        var structType = GeneratorTestHelpers.GetType(compilation, "StructSubject`1").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;
        var nullableType = GeneratorTestHelpers.GetType(compilation, "NullableReferenceSubject`1").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;
        var referenceType = GeneratorTestHelpers.GetType(compilation, "ReferenceSubject`1").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.True(structType.CanBeInstantiated());
        Assert.True(nullableType.CanBeInstantiated());
        Assert.False(referenceType.CanBeInstantiated());
    }

    [Fact]
    public void GetReturnExpression_UsesDefaultForGenericAndNullableBranchCases()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            class Subject<T>
            {
                public T Value => default!;
                public string? NullableValue => default;
            }
            """);

        var genericType = GeneratorTestHelpers.GetType(compilation, "Subject`1").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;
        var nullableType = GeneratorTestHelpers.GetType(compilation, "Subject`1").GetMembers("NullableValue").OfType<IPropertySymbol>().Single().Type;

        Assert.Equal("default", StubDefaults.GetReturnExpression(genericType));
        Assert.Equal("default", StubDefaults.GetReturnExpression(nullableType));
    }

    [Fact]
    public void CanBeInstantiated_RejectsArrayAndUnconstructibleReferenceTypes()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            class Subject
            {
                public int[] ArrayValue => default!;
                public System.Uri UriValue => default!;
            }
            """);

        var arrayType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("ArrayValue").OfType<IPropertySymbol>().Single().Type;
        var uriType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("UriValue").OfType<IPropertySymbol>().Single().Type;

        Assert.False(arrayType.CanBeInstantiated());
        Assert.False(uriType.CanBeInstantiated());
    }

    [Fact]
    public void GetDelegateExpression_ThrowsForUnconstructibleDelegateReturnType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "class Subject { delegate System.Uri Factory(); public Factory Value => default!; }");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.Equal("() => throw new global::System.NotImplementedException()", StubDefaults.GetReturnExpression(returnType));
    }

    [Fact]
    public void CanBeInstantiated_RejectsNonConstructibleReferenceTypes()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "class Subject { public System.Uri Value => default!; }");
        var returnType = GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Value").OfType<IPropertySymbol>().Single().Type;

        Assert.False(returnType.CanBeInstantiated());
    }
}