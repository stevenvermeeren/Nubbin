using Nubbin.Test.UsageTests.Bases;

namespace Nubbin.Test.UsageTests;

[Stub]
public partial class InterfaceStub : InterfaceSample
{
}

[Stub]
public partial class FullyImplementedInterfaceStub : IFullyImplementedInterfaceSample
{
    public int Existing() => 42;

    public string Missing() => "implemented";
}

public class InterfaceStubTests
{
    [Fact]
    public void StubbedInterfaceMethodReturnsDefault()
    {
        Assert.Equal(0, new InterfaceStub().Compare(1, 2));
    }

    [Fact]
    public void StubbedInterfacePropertyUsesDefaultValue()
    {
        var stub = new InterfaceStub();

        Assert.Null(stub.Name);
        stub.Name = "updated";
        Assert.Equal("updated", stub.Name);
    }

    [Fact]
    public void StubbedOneSidedInterfacePropertiesExposeBothAccessors()
    {
        var stub = new InterfaceStub();

        Assert.Null(stub.GetterOnly);
        stub.GetterOnly = "updated";
        Assert.Equal("updated", stub.GetterOnly);

        stub.SetterOnly = 42;
        Assert.Equal(42, stub.SetterOnly);
    }

    [Fact]
    public void StubbedCollectionMembersUseEmptyCompatibleCollections()
    {
        var stub = new InterfaceStub();

        Assert.Empty(stub.Items());
        Assert.Empty(stub.Numbers);
        Assert.Empty(stub.Counts());
        Assert.Empty(stub.Tags);
    }

}