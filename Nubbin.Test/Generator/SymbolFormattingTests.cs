// using Microsoft.CodeAnalysis;
// using Nubbin.Generator;
// using Nubbin.Generator.Emitting;

// namespace Nubbin.Test.Generator;

// public class SymbolFormattingTests
// {
//     [Fact]
//     public void FormatsInterfaceMethodsAndPropertiesAsPublicMembers()
//     {
//         const string source = "interface IContract { int Read(in string value); string Name { get; set; } }";
//         var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "IContract");
//         var method = type.GetMembers("Read").OfType<IMethodSymbol>().Single();
//         var property = type.GetMembers("Name").OfType<IPropertySymbol>().Single();

//         Assert.Equal("public int Read(in string value)", SymbolFormatting.GetMethodDeclaration(method, Stubbable.FromINamedTypeSymbol(type)));
//         Assert.Equal("public string Name", SymbolFormatting.GetPropertyDeclaration(property, Stubbable.FromINamedTypeSymbol(type)));
//         Assert.True(SymbolFormatting.HasGetter(property));
//         Assert.True(SymbolFormatting.HasSetter(property));
//     }

//     [Fact]
//     public void IdentifiesStorageRequiredForOneSidedAbstractProperties()
//     {
//         const string source = "abstract class Base { public abstract int Value { get; } } class Subject : Base { }";
//         var type = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Subject");
//         var property = GeneratorTestHelpers.GetType(GeneratorTestHelpers.CreateCompilation(source), "Base")
//             .GetMembers("Value").OfType<IPropertySymbol>().Single();

//         Assert.True(SymbolFormatting.RequiresPropertyStorage(property));
//         Assert.Equal("internal", SymbolFormatting.GetGeneratedTypeAccessibility(Stubbable.FromINamedTypeSymbol(type)));
//     }
// }