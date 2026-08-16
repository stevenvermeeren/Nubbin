using Nubbin.Generator.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class NamespaceEmitterTests
{
    [Fact]
    public void WithNamespace_WrapsContentsInNamespace()
    {
        var builder = new IndentedStringBuilder();

        builder.WithNamespace("Example.Test", () =>
        {
            builder.AppendLine("class A {}");
        });

        var result = builder.ToString();

        Assert.Contains("namespace Example.Test", result);
        Assert.Contains("class A {}", result);
        Assert.Contains("}", result);
    }
}
