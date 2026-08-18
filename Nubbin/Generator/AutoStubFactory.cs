using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nubbin.Generator.Emitting;

namespace Nubbin.Generator;

internal class AutoStubFactory
{
    private readonly HashSet<string> _generatedStubs = [];
    private readonly SourceProductionContext _productionContext;
    private readonly GeneratorSyntaxContext _syntaxContext;

    public AutoStubFactory(SourceProductionContext productionContext, GeneratorSyntaxContext syntaxContext)
    {
        _productionContext = productionContext;
        _syntaxContext = syntaxContext;
    }

    public void Generate(CompilationUnitSyntax unit)
    {
        foreach (var containingClass in unit.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            GenerateAutoStubsForClass(containingClass);
        }
    }

    private void GenerateAutoStubsForClass(ClassDeclarationSyntax containingClass)
    {
        var stubs = FindAutoStubs(containingClass);
        if (!stubs.Any())
            return;

        var containerTypeInfo = _syntaxContext.SemanticModel.GetDeclaredSymbol(containingClass);
        var containerTypeSymbol = containerTypeInfo ?? throw new InvalidOperationException();

        if (!_productionContext.CheckPartial(
                containerTypeSymbol,
                containingClass.Identifier.GetLocation,
                Diagnostics.PartialAutoStubError))
            return;

        foreach (var stub in stubs)
        {
            var (file, source) = GenerateAutoStub(containerTypeSymbol, stub);
            if (source is not null)
                _productionContext.AddSource(file, source.ToString());
        }

        var container = AutoStubContainerEmitter.Emit(_syntaxContext, containerTypeSymbol, stubs);
        _productionContext.AddSource(containerTypeSymbol.Name + ".AutoStubContainer.g.cs", container.ToString());
    }

    private (string File, string? Source) GenerateAutoStub(
        INamedTypeSymbol containerTypeSymbol,
        MemberAccessExpressionSyntax s)
    {
        var stubType = ((GenericNameSyntax)s.Name).TypeArgumentList.Arguments.Single();
        var typeInfo = _syntaxContext.SemanticModel.GetTypeInfo(stubType);
        var namedTypeSymbol = typeInfo.Type as INamedTypeSymbol
            ?? throw new InvalidOperationException();

        var name = namedTypeSymbol.GetStubTypeName();
        var file = name + ".AutoStub.g.cs";
        if (!_generatedStubs.Add(file))
            return (file, null);

        var _namespace = "Nubbin.Generated";
        if (!namedTypeSymbol.ContainingNamespace.IsGlobalNamespace) 
            _namespace += "." + namedTypeSymbol.ContainingNamespace;
        var stub = new StubDefinition
        {
            Name = name,
            Accessibility = Accessibility.Internal,
            Namespace = _namespace,
            BaseType = namedTypeSymbol,
            AllInterfaces = [namedTypeSymbol, ..namedTypeSymbol.AllInterfaces],
            ContainingAssembly = containerTypeSymbol.ContainingAssembly
        };

        return (file, StubSourceEmitter.Emit(stub));
    }

    private static string CreateStubKey(INamedTypeSymbol type)
    {
        if (type.ContainingType is INamedTypeSymbol parent)
            return $"{CreateStubKey(parent)}_{type.Name}";
        return type.Name;
    }

    private static IEnumerable<MemberAccessExpressionSyntax> FindAutoStubs(SyntaxNode node)
    {
        return node.DescendantNodes()
            .Where(n =>
                n is MemberAccessExpressionSyntax stx
                && stx.Name is GenericNameSyntax name
                && stx.Expression is IdentifierNameSyntax expr
                && name.Identifier.Text == nameof(Stub.Auto)
                && name.TypeArgumentList.Arguments.Count == 1
                && expr.Identifier.Text == nameof(Stub))
            .Cast<MemberAccessExpressionSyntax>();
    }
}