using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;
using Nubbin.Test.Generator;

namespace Nubbin.Test.Generator.Discovery;

public class MissingMethodTests
{
    [Fact]
    public void MissingMethod_Conflicts_WhenGenericConstraintDiffers()
    {
        const string source = """
            public interface IFirst
            {
                bool Invoke<T>() where T : class;
            }

            public interface ISecond
            {
                bool Invoke<T>() where T : struct;
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IFirst"));
        var first = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IFirst").GetMembers("Invoke").OfType<IMethodSymbol>().Single(), type);
        var second = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "ISecond").GetMembers("Invoke").OfType<IMethodSymbol>().Single(), type);

        Assert.True(first.ConflictsWith(second));
        Assert.True(second.ConflictsWith(first));
    }

    [Theory]
    [InlineData("bool Invoke<T>() where T : new();", "bool Invoke<T>();")]
    [InlineData("bool Invoke<T>() where T : notnull;", "bool Invoke<T>();")]
    [InlineData("bool Invoke<T>() where T : class;", "bool Invoke<T>() where T : struct;")]
    [InlineData("bool Invoke<T>() where T : struct;", "bool Invoke<T>() where T : class;")]
    [InlineData("bool Invoke<T>() where T : System.Collections.Generic.IEnumerable<int>;", "bool Invoke<T>() where T : System.Collections.Generic.IReadOnlyCollection<int>;")]
    public void MissingMethod_ReportsEachGenericConstraintMismatchBranch(string firstMethod, string secondMethod)
    {
        var source = $$"""
            public interface IFirst
            {
                {{firstMethod}}
            }

            public interface ISecond
            {
                {{secondMethod}}
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IFirst"));
        var first = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IFirst").GetMembers("Invoke").OfType<IMethodSymbol>().Single(), type);
        var second = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "ISecond").GetMembers("Invoke").OfType<IMethodSymbol>().Single(), type);

        Assert.True(first.ConflictsWith(second));
        Assert.True(second.ConflictsWith(first));
    }

    [Fact]
    public void MissingMethod_DoesNotConflict_WhenGenericArityDiffers()
    {
        const string source = """
            public interface IFirst
            {
                bool Invoke<T>();
            }

            public interface ISecond
            {
                bool Invoke<T1, T2>();
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IFirst"));
        var first = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IFirst").GetMembers("Invoke").OfType<IMethodSymbol>().Single(), type);
        var second = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "ISecond").GetMembers("Invoke").OfType<IMethodSymbol>().Single(), type);

        Assert.False(first.ConflictsWith(second));
        Assert.False(second.ConflictsWith(first));
    }

    [Fact]
    public void MissingMethod_Conflicts_WhenReturnTypeDiffers_AndEquivalentSignatureStillImplements()
    {
        const string source = """
            public interface IFirst
            {
                string Name();
            }

            public interface ISecond
            {
                int Name();
            }

            public interface IThird
            {
                string Name();
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IFirst"));
        var first = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IFirst").GetMembers("Name").OfType<IMethodSymbol>().Single(), type);
        var second = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "ISecond").GetMembers("Name").OfType<IMethodSymbol>().Single(), type);
        var third = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IThird").GetMembers("Name").OfType<IMethodSymbol>().Single(), type);

        Assert.True(first.ConflictsWith(second));
        Assert.False(first.Implements(second));
        Assert.True(first.Implements(third));
    }

    [Fact]
    public void MissingMethod_ParameterMismatchIsDetectedByImplements_NotByConflicts()
    {
        const string source = """
            public interface IFirst
            {
                int Map<T>(T value);
            }

            public interface ISecond
            {
                int Map<T>(string value);
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IFirst"));
        var first = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IFirst").GetMembers("Map").OfType<IMethodSymbol>().Single(), type);
        var sameSignature = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IFirst").GetMembers("Map").OfType<IMethodSymbol>().Single(), type);
        var second = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "ISecond").GetMembers("Map").OfType<IMethodSymbol>().Single(), type);

        Assert.True(first.Implements(sameSignature));
        Assert.False(first.ConflictsWith(second));
        Assert.False(first.Implements(second));
    }

    [Fact]
    public void MissingMethod_RecognizesSameSignatureAndConflictsWithOtherKinds()
    {
        const string source = """
            public interface IMethodContract
            {
                int Value();
            }

            public interface IPropertyContract
            {
                int Value { get; }
            }

            public interface IEventContract
            {
                event System.EventHandler Value;
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IMethodContract"));
        var method = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IMethodContract").GetMembers("Value").OfType<IMethodSymbol>().Single(), type);
        var sameSignature = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IMethodContract").GetMembers("Value").OfType<IMethodSymbol>().Single(), type);
        var property = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IPropertyContract").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);
        var @event = new MissingEvent(GeneratorTestHelpers.GetType(compilation, "IEventContract").GetMembers("Value").OfType<IEventSymbol>().Single(), type);

        Assert.True(method.Implements(sameSignature));
        Assert.True(method.ConflictsWith(property));
        Assert.True(method.ConflictsWith(@event));
    }
}
