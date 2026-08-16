using System.Diagnostics.CodeAnalysis;

namespace Nubbin;

/// <summary>
/// Placeholder type to enable auto-stubs.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Stub
{
    /// <summary>
    /// Returns a stub with default behavior for given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Auto<T>()
    {
        throw new NotImplementedException(
        "Placeholder for auto-stubs for Intellisense. " +
        "Should be superceded by the generated code and never invoked directly.");
    }
}