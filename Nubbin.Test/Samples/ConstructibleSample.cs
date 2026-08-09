namespace Nubbin.Test.Samples;

public sealed class DefaultConstructible
{
    public int Value { get; } = 42;
}

public interface IConstructibleSample
{
    DefaultConstructible Create();
    Task<DefaultConstructible> CreateAsync();
}

[Stub]
public partial class ConstructibleStub : IConstructibleSample
{
}