using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Nubbin.Test.Generator;

internal static class GeneratorTestHelpers
{
    public static Compilation CreateCompilation(string source, NullableContextOptions nullableContext = NullableContextOptions.Enable)
        => CreateCompilation([source], nullableContext);

    public static Compilation CreateCompilation(string[] sources, NullableContextOptions nullableContext = NullableContextOptions.Enable)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(Nubbin.StubAttribute).Assembly.Location)
            ])
            .ToArray();

        return CSharpCompilation.Create(
            assemblyName: "GeneratorTestAssembly",
            syntaxTrees: sources.Select(s => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.Latest))),
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContext));
    }

    public static INamedTypeSymbol GetType(Compilation compilation, string metadataName)
    {
        return compilation.GetTypeByMetadataName(metadataName)
            ?? throw new InvalidOperationException($"Type '{metadataName}' was not found.");
    }
}