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
            public partial class Consumer
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
    public void IgnoresNonStubAutoCalls()
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
                    var value = SomeOtherAuto<IComponent>();
                    _ = value;
                }
            }

            static class SomeOtherAuto<T>
            {
                public static T Invoke() => default!;
            }
            """;

        var compilation = GeneratorTestHelpers.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AutoStubGenerator());

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var generatedSource = string.Join(
            Environment.NewLine,
            driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources)
                .Select(sourceText => sourceText.SourceText.ToString()));

        Assert.DoesNotContain("public static T Auto<T>()", generatedSource);
    }

    [Fact]
    public void ReportsDiagnosticWhenAutoStubContainerIsNotPartial()
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
            diagnostic => diagnostic.Id == Diagnostics.PartialAutoStubError.Id
                && diagnostic.Location.SourceSpan.Start == 74
                && diagnostic.Location.SourceSpan.End == 82);
    }
}
