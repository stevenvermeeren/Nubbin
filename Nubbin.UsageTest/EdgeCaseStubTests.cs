using Nubbin.UsageTests.Bases;

namespace Nubbin.UsageTests;

[Stub]
public partial class PartialInterfaceStub : IPartialInterfaceSample
{
    public int Existing() => 42;
}

[Stub]
public partial class UnsupportedReferenceStub : IUnsupportedReferenceSample
{
}

[Stub]
public partial class ObliviousReferenceStub : IObliviousReferenceSample
{
}

[Stub]
public partial class ObliviousOutParameterStub : IObliviousOutParameterSample
{
}

public class EdgeCaseStubTests
{
    [Fact]
    public void PartiallyImplementedInterfaceOnlyStubsMissingMembers()
    {
        var stub = new PartialInterfaceStub();

        Assert.Equal(42, stub.Existing());
        Assert.Null(stub.Missing());
    }

    [Fact]
    public void PartiallyImplementedBaseOnlyStubsMissingMembers()
    {
        var stub = new PartialBaseStub();

        Assert.Equal(42, stub.Existing());
        Assert.Null(stub.Missing());
    }

    [Fact]
    public void FullyImplementedMembersNeedNoGeneratedMembers()
    {
        Assert.Equal("implemented", new FullyImplementedInterfaceStub().Missing());
        Assert.Equal("implemented", new FullyImplementedBaseStub().Missing());
    }

    [Fact]
    public void UnsupportedNonNullableReferenceThrows()
    {
        Assert.Throws<NotImplementedException>(() => new UnsupportedReferenceStub().Missing());
    }

    [Fact]
    public void NullableObliviousReferenceRetainsNullDefault()
    {
        Assert.Null(new ObliviousReferenceStub().Missing());
    }

    [Fact]
    public void NullableObliviousOutParameterRetainsNullDefault()
    {
        new ObliviousOutParameterStub().Read(out var value);

        Assert.Null(value);
    }
}
