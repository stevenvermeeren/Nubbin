using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Discovery;

namespace Nubbin.Analyzer.Emitting;

internal static class StubSourceEmitter
{
    public static string Emit(INamedTypeSymbol type)
    {
        return Emit(StubDefinition.FromINamedTypeSymbol(type));
    }

    public static string Emit(StubDefinition type)
    {
        var membersToGenerate = FindMembersToGenerate(type);

        var storageProperties = membersToGenerate.OfType<MissingProperty>()
            .Where(p => p.RequiresPropertyStorage);

        var builder = new IndentedStringBuilder();
        builder.WithNamespace(type.Namespace, () =>
        {
            builder.WithContainingTypes(type.ContainingType, () =>
            {
                builder.WithClass(
                    new ClassEmitter.Definition(type)
                    {
                        IsPartial = type.LeafType is not null,
                        BaseTypes = type.LeafType is not null || type.BaseType is null ? [] : [type.BaseType]
                    },
                    () =>
                    {
                        foreach (var member in membersToGenerate)
                        {
                            member.Output(builder);
                        }

                        if (storageProperties.Count() > 0)
                        {
                            builder.AppendLine();
                            builder.AppendPropertyStorage(type, storageProperties.Select(p => p.Property));
                        }
                    });
            });
        });

        if (storageProperties.Count() > 0 && IsAccessible(type.LeafType ?? type.BaseType!))
        {
            builder.WithNamespace("Nubbin", () =>
            {
                builder.AppendPropertyStorageLookup(type);
            });
        }

        return builder.ToString();
    }

    private static MissingMember[] FindMembersToGenerate(StubDefinition type)
    {
        var methods = new MethodDiscoverer().GetSymbolsMissingImplementation(type);
        var events = new EventDiscoverer().GetSymbolsMissingImplementation(type);
        var properties = new PropertyDiscoverer().GetSymbolsMissingImplementation(type).ToArray();

        var allMembers = methods.Concat(events).Concat(properties)
            .OrderBy(m => m.Symbol.ContainingType.TypeKind == TypeKind.Interface
                ? m.Symbol.ContainingType.AllInterfaces.Length
                : int.MaxValue)
            .ToArray();

        foreach (var member in allMembers)
        {
            member.Process(allMembers);
        }
        return allMembers.Where(m => m.ShouldEmit).ToArray();
    }

    private static bool IsAccessible(INamedTypeSymbol type)
    {
        if (type.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected)
            return false;
        if (type.ContainingType is INamedTypeSymbol parent)
            return IsAccessible(parent);
        return true;
    }

    private static void WithContainingTypes(
        this IndentedStringBuilder builder,
        INamedTypeSymbol? type,
        Action emitContents)
    {
        if (type is not null)
        {
            builder.WithContainingTypes(type.ContainingType, () =>
            {
                builder.WithClass(new ClassEmitter.Definition(type) { IsPartial = true }, emitContents);
            });
        }
        else {
            emitContents();
        }
    }
}