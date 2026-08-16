using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class SymbolFormattingTests
{
    [Fact]
    public void GetsStubTypeNameFromTypeSyntax()
    {
        var syntax = SyntaxFactory.ParseTypeName("IComponent");

        Assert.Equal("IComponentStub", syntax.GetStubTypeName());
    }

    [Fact]
    public void MapsAccessibilityToCSharpKeyword()
    {
        Assert.Equal("public", Accessibility.Public.AsTypeAccessibility());
        Assert.Equal("internal", Accessibility.Internal.AsTypeAccessibility());
        Assert.Equal("private", Accessibility.Private.AsTypeAccessibility());
    }
}
