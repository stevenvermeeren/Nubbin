namespace Nubbin.Test.Samples;

public abstract class AbstractSample
{
    public abstract string Create(int value);

    public abstract object Value { get; set; }

    public abstract object GetterOnly { get; }
    public abstract int SetterOnly { set; }

    protected abstract int CreateProtected(int value);
}

[Stub]
public partial class AbstractStub : AbstractSample
{
    public int CallProtected(int value) => CreateProtected(value);

    public object GetPropertyHelper() => this;
}