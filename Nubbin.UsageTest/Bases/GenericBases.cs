namespace Nubbin.UsageTest.Bases;

public abstract class GenericSample<T>
{
    public abstract T Property { get; }
    public abstract void Method(T param);
}

public abstract class ConstrainedGenericSample<T>
    where T : IComparable, new()
{
    public abstract T Property { get; }
    public abstract void Method(T param);
}

public abstract class GenericMembersSample
{
    public abstract void GenericMethod<T>(T param);
    public abstract void ConstrainedGenericMethod<T>(T param) where T : IComparable;
}

public interface IContravariantSample<in T>
{
    public abstract void Method(T param);
}

public interface ICovariantSample<out T> where T : struct
{
    public abstract T Method();
}