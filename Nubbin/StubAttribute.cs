namespace Nubbin;

/// <summary>
/// Marks a partial class for dummy implementation generation.
/// </summary>
/// <remarks>
/// The source generator creates implementations for unimplemented abstract
/// members inherited from base classes and members required by implemented
/// interfaces. Generated methods return default values or completed tasks, and
/// common collection interfaces receive empty compatible collections.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StubAttribute : Attribute
{
}
