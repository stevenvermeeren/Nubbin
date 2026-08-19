using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nubbin.Generator.Emitting;

namespace Nubbin.Generator;

internal class AutoStubFactory
{
    private readonly Dictionary<string, INamedTypeSymbol> _generatedStubs;
    private readonly SourceProductionContext _productionContext;
    private readonly GeneratorSyntaxContext _syntaxContext;

    public AutoStubFactory(
        SourceProductionContext productionContext,
        GeneratorSyntaxContext syntaxContext,
        Dictionary<string, INamedTypeSymbol> generatedStubs)
    {
        _productionContext = productionContext;
        _syntaxContext = syntaxContext;
        _generatedStubs = generatedStubs;
    }

    public void Process(CompilationUnitSyntax unit)
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

        var containerTypeSymbol = _syntaxContext.SemanticModel.GetDeclaredSymbol(containingClass)
            ?? throw new InvalidOperationException("Unable to find TypeSymbol for class");

        if (HasDiagnosticError(containerTypeSymbol, containingClass))
            return;

        foreach (var stub in stubs)
        {
            var (file, source) = GenerateAutoStub(containerTypeSymbol, stub);
            if (source is not null)
                _productionContext.AddSource(file, source.ToString());
        }
    }

    private (string File, string? Source) GenerateAutoStub(
        INamedTypeSymbol containerTypeSymbol,
        MemberAccessExpressionSyntax s)
    {
        var stubType = ((GenericNameSyntax)s.Name).TypeArgumentList.Arguments.Single();
        var typeInfo = _syntaxContext.SemanticModel.GetTypeInfo(stubType);
        var namedTypeSymbol = typeInfo.Type as INamedTypeSymbol
            ?? throw new InvalidOperationException();

        var file = $"{namedTypeSymbol.GetStubTypeNameWithNamespace()}.AutoStub.g.cs";
        if (_generatedStubs.ContainsKey(file))
            return (file, null);
        _generatedStubs.Add(file, namedTypeSymbol);

        var _namespace = "Nubbin.Generated";
        if (!namedTypeSymbol.ContainingNamespace.IsGlobalNamespace) 
            _namespace += "." + namedTypeSymbol.ContainingNamespace;
        var stub = new StubDefinition
        {
            Name = namedTypeSymbol.GetStubTypeName(),
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
                && name.Identifier.Text == "Auto"
                && name.TypeArgumentList.Arguments.Count == 1
                && expr.Identifier.Text == nameof(Stub))
            .Cast<MemberAccessExpressionSyntax>();
    }

    private bool HasDiagnosticError(INamedTypeSymbol containerTypeSymbol, ClassDeclarationSyntax containingClass)
    {
        var res = false;

        var containingNamespace = containerTypeSymbol.ContainingNamespace.ToDisplayString();
        if (!containingNamespace.Equals("Nubbin")
            && !containingNamespace.StartsWith("Nubbin.")
            && !containingClass.SyntaxTree.GetCompilationUnitRoot().Usings.Any(u => u.Name?.ToString() == "Nubbin"))
        {
            _productionContext.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.AutoStubMissingUsingError,
                    containingClass.Identifier.GetLocation(),
                    containerTypeSymbol.Name));
            res = true;
        }

        return res;
    }
}