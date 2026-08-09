namespace Nubbin.Test.Samples;

public interface IHybridSample
{
    bool IsValid(string value);
}

public abstract class HybridBase
{
    public abstract string Create(int value);
}

[Stub]
public partial class HybridStub : HybridBase, IHybridSample
{
}