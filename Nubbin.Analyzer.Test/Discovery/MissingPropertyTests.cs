using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;
using Nubbin.Test.Generator;

namespace Nubbin.Test.Generator.Discovery;

public class MissingPropertyTests
{
    [Fact]
    public void MissingProperty_RequiresStorage_WhenOnlyOneAccessorExists()
    {
        const string source = """
            public interface IContract
            {
                string Value { set; }
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var property = GeneratorTestHelpers.GetType(compilation, "IContract").GetMembers("Value").OfType<IPropertySymbol>().Single();
        var member = new MissingProperty(property, StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IContract")));

        Assert.True(member.UsePropertyStorage);
    }

    [Fact]
    public void MissingProperty_RecognizesSameSignatureAndConflictsWithOtherKinds()
    {
        const string source = """
            public interface IPropertyContract
            {
                int Value { get; }
            }

            public interface IMethodContract
            {
                int Value();
            }

            public interface IEventContract
            {
                event System.EventHandler Value;
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IPropertyContract"));
        var property = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IPropertyContract").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);
        var sameProperty = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IPropertyContract").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);
        var method = new MissingMethod(GeneratorTestHelpers.GetType(compilation, "IMethodContract").GetMembers("Value").OfType<IMethodSymbol>().Single(), type);
        var @event = new MissingEvent(GeneratorTestHelpers.GetType(compilation, "IEventContract").GetMembers("Value").OfType<IEventSymbol>().Single(), type);

        Assert.True(property.Implements(sameProperty));
        Assert.True(property.ConflictsWith(method));
        Assert.True(property.ConflictsWith(@event));
    }

    [Theory]
    [InlineData("int Value { get; }", "int Value { get; }", true)]
    [InlineData("int Value { get; }", "long Value { get; }", false)]
    [InlineData("int Value { get; }", "int Value { set; }", false)]
    [InlineData("int Value { set; }", "int Value { get; }", false)]
    [InlineData("int Value { get; set; }", "int Value { get; }", true)]
    [InlineData("int Value { get; }", "int Value { get; set; }", false)]
    public void MissingProperty_Implements_OnlyWhenTypeAndAccessorShapeMatch(string firstProperty, string secondProperty, bool expected)
    {
        var source = $$"""
            public interface IFirst
            {
                {{firstProperty}}
            }

            public interface ISecond
            {
                {{secondProperty}}
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "IFirst"));
        var first = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IFirst").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);
        var second = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "ISecond").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);

        Assert.Equal(expected, first.Implements(second));
    }
}
