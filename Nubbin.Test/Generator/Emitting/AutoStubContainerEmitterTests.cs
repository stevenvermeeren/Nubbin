using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nubbin.Generator;

namespace Nubbin.Test.Generator.Emitting;

public class AutoStubContainerEmitterTests
{
    [Fact]
    public void EmitsFactoryLookupForRequestedInterfaceStub()
    {
        const string source = """
            public interface IComponent
            {
                int Value { get; set; }
            }

            public partial class Consumer
            {
                public void Test()
                {
                    var component = Stub.Auto<IComponent>();
                }
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AutoStubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var generatedSource = string.Join(
            Environment.NewLine,
            driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources)
                .Select(sourceText => sourceText.SourceText.ToString()));

        Assert.Contains("public static T Auto<T>()", generatedSource);
        Assert.Contains("typeof(T) == typeof(global::IComponent)", generatedSource);
        Assert.Contains("new global::Nubbin.Generated.IComponentStub()", generatedSource);
    }
}
