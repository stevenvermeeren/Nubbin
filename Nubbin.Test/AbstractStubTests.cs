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