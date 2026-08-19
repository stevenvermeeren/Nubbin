using Nubbin.UsageTests.Bases;

namespace Nubbin.UsageTests;

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

public partial class AutoInterfaceConsumer
{
    public static InterfaceSample CreateDefault() => Stub.Auto<InterfaceSample>();
}

public abstract class AutoBaseSample
{
    public abstract int Value { get; set; }
}

public partial class AutoBaseConsumer
{
    public static AutoBaseSample CreateDefault() => Stub.Auto<AutoBaseSample>();
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

    [Fact]
    public void AutoStubFactoryCreatesDefaultImplementation()
    {
        var stub = AutoInterfaceConsumer.CreateDefault();

        Assert.Equal(0, stub.Compare(1, 2));
        Assert.Null(stub.Name);
        stub.Name = "updated";
        Assert.Equal("updated", stub.Name);
    }

    [Fact]
    public void AutoStubFactoryCreatesDefaultBaseImplementation()
    {
        var stub = AutoBaseConsumer.CreateDefault();

        Assert.Equal(0, stub.Value);
        stub.Value = 42;
        Assert.Equal(42, stub.Value);
    }

}