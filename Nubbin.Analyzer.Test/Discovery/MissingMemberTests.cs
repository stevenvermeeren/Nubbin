using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;
using Nubbin.Test.Generator;

namespace Nubbin.Test.Generator.Discovery;

public class MissingMemberTests
{
    [Fact]
    public void Process_MarksMemberAsExplicitImplementation_WhenConflictingMemberExists()
    {
        const string source = """
            public interface IOne
            {
                int Value { get; }
            }

            public interface ITwo
            {
                string Value { get; }
            }

            public class Target : IOne, ITwo
            {
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Target"));
        var one = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IOne").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);
        var two = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "ITwo").GetMembers("Value").OfType<IPropertySymbol>().Single(), type);

        one.Process([one, two]);

        Assert.True(one.ImplementExplicitly);
        Assert.True(one.ShouldEmit);
    }

    [Fact]
    public void Process_DropsMember_WhenAnotherMemberImplementsIt()
    {
        const string source = """
            public interface IBase
            {
                string Name { get; }
            }

            public interface IDerived : IBase
            {
                string Name { get; }
            }

            public class Target : IBase, IDerived
            {
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Target"));
        var baseMember = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IBase").GetMembers("Name").OfType<IPropertySymbol>().Single(), type);
        var derivedMember = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "IDerived").GetMembers("Name").OfType<IPropertySymbol>().Single(), type);

        baseMember.Process([baseMember, derivedMember]);

        Assert.False(baseMember.ShouldEmit);
    }

    [Fact]
    public void Process_LeavesNonInterfaceMembersUntouched()
    {
        const string source = """
            public class Subject
            {
                public virtual string Name => string.Empty;
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Subject"));
        var property = new MissingProperty(GeneratorTestHelpers.GetType(compilation, "Subject").GetMembers("Name").OfType<IPropertySymbol>().Single(), type);

        property.Process([property]);

        Assert.True(property.ShouldEmit);
        Assert.False(property.ImplementExplicitly);
    }
}
