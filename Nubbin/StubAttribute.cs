namespace Nubbin;

/// <summary>
/// Marks a partial class for dummy implementation generation.
/// </summary>
/// <remarks>
/// The source generator creates implementations for unimplemented abstract
/// members inherited from base classes and members required by implemented
/// interfaces. Generated methods return default values, completed tasks, or a
/// new instance when the return type has a public parameterless constructor.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StubAttribute : Attribute
{
}
