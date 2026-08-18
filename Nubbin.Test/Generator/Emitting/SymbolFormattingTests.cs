using Microsoft.CodeAnalysis;
using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class SymbolFormattingTests
{
    [Fact]
    public void MapsAccessibilityToCSharpKeyword()
    {
        Assert.Equal("public", Accessibility.Public.AsTypeAccessibility());
        Assert.Equal("internal", Accessibility.Internal.AsTypeAccessibility());
        Assert.Equal("private", Accessibility.Private.AsTypeAccessibility());
    }
}
