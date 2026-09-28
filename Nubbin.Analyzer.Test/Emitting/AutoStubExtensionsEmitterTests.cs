using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nubbin.Analyzer;

namespace Nubbin.Test.Generator.Emitting;

public class AutoStubExtensionsEmitterTests
{
    [Fact]
    public void EmitsFactoryLookupForRequestedInterfaceStub()
    {
        const string source = """
            using Nubbin;
            public interface IComponent
            {
                int Value { get; set; }
            }

            public class Consumer
            {
                public void Test()
                {
                    var component = Stub.Auto<IComponent>();
                    var component = Stub.Auto<System.Action<System.Action>>();
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
        Assert.Contains("typeof(T) == typeof(global::System.Action<global::System.Action>)", generatedSource);
        Assert.Contains("new global::System.Action<global::System.Action>((_) => { })", generatedSource);
    }
}
