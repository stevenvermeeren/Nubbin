using Microsoft.CodeAnalysis;

namespace Nubbin.Test.Generator;

public class SymbolFormattingTests
{
    [Fact]
    public void FormatsInterfaceMethodsAndPropertiesAsPublicMembers()
    {
        const string source = "interface IContract { int Read(in string value); string Name { get; set; } }";
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "IContract");
        var method = type.GetMembers("Read").OfType<IMethodSymbol>().Single();
        var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();

        Assert.Equal("public int Read(in string value)", Nubbin.Generator.SymbolFormatting.GetMethodDeclaration(method, type));
        Assert.Equal("public string Name", Nubbin.Generator.SymbolFormatting.GetPropertyDeclaration(property, type));
        Assert.True(Nubbin.Generator.SymbolFormatting.HasGetter(property));
        Assert.True(Nubbin.Generator.SymbolFormatting.HasSetter(property));
    }

    [Fact]
    public void IdentifiesStorageRequiredForOneSidedAbstractProperties()
    {
        const string source = "abstract class Base { public abstract int Value { get; } } class Subject : Base { }";
        var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");
        var property = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Base")
            .GetMembers("Value").OfType<IPropertySymbol>().Single();

        Assert.True(Nubbin.Generator.SymbolFormatting.RequiresPropertyStorage(property));
        Assert.Equal("internal", Nubbin.Generator.SymbolFormatting.GetGeneratedTypeAccessibility(type));
    }
}