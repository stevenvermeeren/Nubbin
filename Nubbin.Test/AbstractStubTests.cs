using Nubbin.Test.Samples;

namespace Nubbin.Test;

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
        stub.Properties.GetterOnly = "updated";
        Assert.Equal("updated", stub.GetterOnly);

        stub.SetterOnly = 42;
        Assert.Equal(42, stub.Properties.SetterOnly);
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