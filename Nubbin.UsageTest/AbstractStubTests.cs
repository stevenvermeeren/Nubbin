using Nubbin.Test.UsageTests.Bases;

namespace Nubbin.Test.UsageTests;

[Stub]
public partial class AbstractStub : AbstractSample
{
    public int CallProtected(int value) => CreateProtected(value);

    public object GetPropertyHelper() => this;
}

[Stub]
public partial class PartialBaseStub : PartialBaseSample
{
}

[Stub]
public partial class FullyImplementedBaseStub : FullyImplementedBaseSample
{
    public override string Missing() => "implemented";
}

public class AbstractStubTests
{
    [Fact]
    public void StubbedAbstractMethodReturnsDefault()
    {
        Assert.Null(new AbstractStub().Create(1));
    }

    [Fact]
    public void StubbedAbstractPropertyUsesDefaultValue()
    {
        var stub = new AbstractStub();

        Assert.Null(stub.Value);
        stub.Value = "updated";
        Assert.Equal("updated", stub.Value);
    }

    [Fact]
    public void StubbedOneSidedAbstractPropertiesMatchBaseAccessors()
    {
        var stub = new AbstractStub();

        Assert.Null(stub.GetterOnly);
        Stubs.GetPropertyHelper(stub).GetterOnly = "updated";
        Assert.Equal("updated", stub.GetterOnly);

        stub.SetterOnly = 42;
        Assert.Equal(42, Stubs.GetPropertyHelper(stub).SetterOnly);
    }

    [Fact]
    public void StubbedProtectedAbstractMethodReturnsDefault()
    {
        Assert.Equal(0, new AbstractStub().CallProtected(1));
    }

    [Fact]
    public void PartiallyImplementedBaseOnlyStubsAbstractMembers()
    {
        var stub = new PartialBaseStub();

        Assert.Equal(42, stub.Existing());
        Assert.Null(stub.Missing());
    }

    [Fact]
    public void FullyImplementedBaseNeedsNoGeneratedMembers()
    {
        Assert.Equal("implemented", new FullyImplementedBaseStub().Missing());
    }
}