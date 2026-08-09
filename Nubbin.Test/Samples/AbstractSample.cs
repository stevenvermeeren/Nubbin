namespace Nubbin.Test.Samples;

public abstract class AbstractSample
{
    public abstract string Create(int value);

    protected abstract int CreateProtected(int value);
}

[Stub]
public partial class AbstractStub : AbstractSample
{
    public int CallProtected(int value) => CreateProtected(value);
}