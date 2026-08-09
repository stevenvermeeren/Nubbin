using Nubbin.Test.Samples;

namespace Nubbin.Test;

public class InterfaceStubTests
{
    [Fact]
    public void StubbedInterfaceMethodReturnsDefault()
    {
        Assert.Equal(0, new InterfaceStub().Compare(1, 2));
    }

    [Fact]
    public void PartiallyImplementedInterfaceOnlyStubsMissingMembers()
    {
        var stub = new PartialInterfaceStub();

        Assert.Equal(42, stub.Existing());
        Assert.Null(stub.Missing());
    }

    [Fact]
    public void FullyImplementedInterfaceNeedsNoGeneratedMembers()
    {
        Assert.Equal("implemented", new FullyImplementedInterfaceStub().Missing());
    }
}