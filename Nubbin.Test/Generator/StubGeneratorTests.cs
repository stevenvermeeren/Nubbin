using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Nubbin.Generator;

namespace Nubbin.Test.Generator;

public class StubGeneratorTests
{
    [Fact]
    public void GeneratesImplementationForAttributedPartialClass()
    {
        const string source = """
            public abstract class Base
            {
                public abstract int Value { get; set; }
                public abstract string Read();
            }
            [Nubbin.Stub]
            public partial class Subject : Base
            {
            }
            """;
        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new StubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var generatedSource = Assert.Single(driver.GetRunResult().Results.Single().GeneratedSources).SourceText.ToString();

        Assert.Contains("public override int Value", generatedSource);
        Assert.Contains("public override string Read()", generatedSource);
        Assert.Contains("{ get; set; } = default!;", generatedSource);
        Assert.Contains("throw new global::System.NotImplementedException();", generatedSource);
    }

    [Fact]
    public void DoesNotGenerateForUnmarkedClasses()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation("public partial class Subject { }");
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new StubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Empty(driver.GetRunResult().Results.Single().GeneratedSources);
    }
}