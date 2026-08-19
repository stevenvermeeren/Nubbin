using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Nubbin.Generator;

namespace Nubbin.Test.Generator;

public class AutoStubGeneratorTests
{
    [Fact]
    public void FindsAutoStubCallsWithinClassBody()
    {
        const string source = """
            using Nubbin;
            public interface IComponent
            {
                int Value { get; set; }
            }

            public abstract class AbstractComponent
            {
                public abstract int Value { get; set; }
            }

            public partial class Consumer
            {
                public void Test()
                {
                    var interfaceStub = Stub.Auto<IComponent>();
                    var abstractStub = Stub.Auto<AbstractComponent>();
                    _ = interfaceStub.Value + abstractStub.Value;
                }
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AutoStubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var generatedSource = string.Join(
            Environment.NewLine,
            driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources).Select(sourceText => sourceText.SourceText.ToString()));

        Assert.Contains("typeof(T) == typeof(global::IComponent)", generatedSource);
        Assert.Contains("typeof(T) == typeof(global::AbstractComponent)", generatedSource);
        Assert.Contains("new global::Nubbin.Generated.IComponentStub()", generatedSource);
        Assert.Contains("new global::Nubbin.Generated.AbstractComponentStub()", generatedSource);
    }

    [Fact]
    public void CanStubNestedType()
    {
        const string source = """
            using Nubbin;
            public class Consumer
            {
                public interface IComponent
                {
                    int Value { get; set; }
                }

                public void Test()
                {
                    var interfaceStub = Stub.Auto<IComponent>();
                    _ = interfaceStub.Value;
                }
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AutoStubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var generatedSource = string.Join(
            Environment.NewLine,
            driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources).Select(sourceText => sourceText.SourceText.ToString()));

        Assert.Contains("typeof(T) == typeof(global::Consumer.IComponent)", generatedSource);
        Assert.Contains("new global::Nubbin.Generated.Consumer_IComponentStub()", generatedSource);
    }

    [Fact]
    public void CanStubMultipleUnits()
    {
        const string first = """
            using Nubbin;
            namespace First {
                public interface IComponent
                {
                    int Value { get; set; }
                }
                public class Consumer
                {
                    public void Test()
                    {
                        var interfaceStub = Stub.Auto<IComponent>();
                        _ = interfaceStub.Value;
                    }
                }
            }
            """;
        var second = """
            using Nubbin;
            namespace Second {
                public class Consumer
                {
                    public void Test()
                    {
                        var interfaceStub = Stub.Auto<First.IComponent>();
                        _ = interfaceStub.Value;
                    }
                }
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation([first, second]);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AutoStubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var generatedSource = string.Join(
            Environment.NewLine,
            driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources).Select(sourceText => sourceText.SourceText.ToString()));

        Assert.Contains("typeof(T) == typeof(global::First.IComponent)", generatedSource);
        Assert.Contains("new global::Nubbin.Generated.First.IComponentStub()", generatedSource);
    }

    [Fact]
    public void ReportsDiagnostics()
    {
        const string source = """
            public interface IComponent
            {
                int Value { get; set; }
            }

            public class Consumer
            {
                public void Test()
                {
                    var component = Stub.Auto<IComponent>();
                    _ = component.Value;
                }
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AutoStubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Contains(
            driver.GetRunResult().Diagnostics,
            diagnostic => diagnostic.Id == Diagnostics.AutoStubMissingUsingError.Id
                && diagnostic.Location.SourceSpan.Start == 74
                && diagnostic.Location.SourceSpan.End == 82);
    }
}
