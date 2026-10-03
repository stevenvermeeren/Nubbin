namespace Nubbin.UsageTest;

public partial class NestedStubTests
{
    public interface INestedInterface
    {
        bool Value { get; set; }
        string Getter { get; }
    }

    [Stub]
    public partial class NestedStub : INestedInterface;

    [Stub]
    private partial class PrivateNestedStub : INestedInterface;

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

    [Fact]
    public void CanHavePrivateNestedStubClass()
    {
        var stub = new PrivateNestedStub();
        stub.Value = true;
        Assert.True(stub.Value);
        stub.Getter = "string";
        Assert.Equal("string", stub.Getter);
    }
}