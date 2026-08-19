namespace Nubbin.UsageTests.Bases;

public abstract class AbstractSample
{
    public abstract string? Create(int value);
    public abstract object? Value { get; set; }
    public abstract object? GetterOnly { get; }
    public abstract int SetterOnly { set; }
    protected abstract int CreateProtected(int value);
}

public abstract class PartialBaseSample
{
    public abstract string? Missing();
    public virtual int Existing() => 42;
}

public abstract class FullyImplementedBaseSample
{
    public abstract string Missing();
}
