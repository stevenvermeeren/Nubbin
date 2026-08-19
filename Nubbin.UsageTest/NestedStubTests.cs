namespace Nubbin.Test.UsageTests;

public partial class NestedStubTests
{
    public interface INestedInterface
    {
        bool Value { get; set; }
    }

    [Stub]
    public partial class NestedStub : INestedInterface;

    [Fact]
    public void CanStubNestedInterface()
    {
        var stub = Stub.Auto<INestedInterface>();
        stub.Value = true;
        Assert.True(stub.Value);
    }

    [Fact]
    public void CanHaveNestedStubClass()
    {
        var stub = new NestedStub();
        stub.Value = true;
        Assert.True(stub.Value);
    }
}