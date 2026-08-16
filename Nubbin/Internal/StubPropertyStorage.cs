using System.Runtime.CompilerServices;

namespace Nubbin.Internal;

/// <summary>
/// Stores strongly typed generated property state without adding storage members to stub types.
/// Not intended for direct usage by consumers, but public as it has to be callable from the generated
/// code in other assemblies.
/// </summary>
public static class StubPropertyStorage<TStub, TProperties>
    where TStub : class
    where TProperties : class, new()
{
    private static readonly ConditionalWeakTable<TStub, TProperties> Values = new();

    /// <summary>
    /// Gets the property storage associated with a stub instance.
    /// </summary>
    /// <param name="owner">The stub instance that owns the properties.</param>
    /// <returns>The typed property storage for <paramref name="owner"/>.</returns>
    public static TProperties Get(TStub owner)
    {
        return Values.GetValue(owner, static _ => new TProperties());
    }
}