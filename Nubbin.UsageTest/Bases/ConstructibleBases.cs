namespace Nubbin.UsageTests.Bases;

public sealed class DefaultConstructible
{
    public int Value { get; } = 42;
}

public interface IConstructibleSample
{
    DefaultConstructible Create();
    Task<DefaultConstructible> CreateAsync();
}
