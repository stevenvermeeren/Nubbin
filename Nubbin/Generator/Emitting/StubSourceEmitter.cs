using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Nubbin.Generator.Discovery;

namespace Nubbin.Generator.Emitting;

internal static class StubSourceEmitter
{
    public static string Emit(INamedTypeSymbol type)
    {
        return Emit(StubDefinition.FromINamedTypeSymbol(type));
    }

    public static string Emit(StubDefinition type)
    {
        var properties = new PropertyDiscoverer().GetSymbolsMissingImplementation(type).ToArray();
        var storageProperties = properties.Where(PropertyEmitter.RequiresPropertyStorage).ToArray();

        var builder = new IndentedStringBuilder();
        builder.WithNamespace(type.Namespace, () =>
        {
            builder.WithClass(
                new ClassEmitter.Definition(type)
                {
                    IsPartial = type.LeafType is not null,
                    BaseTypes = type.BaseType is null ? [] : [type.BaseType]
                },
                () =>
                {
                    foreach (var method in new MethodDiscoverer().GetSymbolsMissingImplementation(type))
                    {
                        builder.AppendMethod(method, type);
                    }

                    foreach (var _event in new EventDiscoverer().GetSymbolsMissingImplementation(type))
                    {
                        builder.AppendEvent(type, _event);
                    }

                    foreach (var property in properties)
                    {
                        builder.AppendProperty(type, property, storageProperties.Contains(property, SymbolEqualityComparer.Default));
                    }
                });
        });

        if (storageProperties.Length > 0)
        {
            builder.AppendPropertyStorage(type, storageProperties);
        }

        return builder.ToString();
    }
}