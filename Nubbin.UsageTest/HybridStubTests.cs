using Nubbin.UsageTest.Bases;

namespace Nubbin.UsageTest;

[Stub]
public partial class HybridStub : HybridBase, IHybridSample
{
}

[Stub]
public partial class SameMemberStub : SameMemberBaseSample, ISameMemberSample
{
}

public class HybridStubTests
{
    [Fact]
    public void StubbedHybridMethodsReturnDefaults()
    {
        var stub = new HybridStub();

        Assert.Null(stub.Create(1));
        Assert.False(stub.IsValid("value"));

        stub.GetPropertyHelper().GetterOnly = true;
        Assert.True(stub.GetterOnly);
    }

    [Fact]
    public void SameMemberDeclaredByBaseAndInterfaceIsGeneratedOnce()
    {
        Assert.Equal(0, new SameMemberStub().Calculate(1));
    }
}