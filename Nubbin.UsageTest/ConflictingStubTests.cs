using Nubbin.UsageTest.Bases;

namespace Nubbin.UsageTest;

[Stub]
public partial class Confliting1Stub : ConflictingBase, IConflictingInterface1;

[Stub]
public partial class Confliting2Stub : ConflictingBase, IConflictingInterface2;

[Stub]
public partial class Confliting3Stub : ConflictingBase, IConflictingInterface3;

[Stub]
public partial class ConflitingAllStub : ConflictingBase, IConflictingInterface1, IConflictingInterface2, IConflictingInterface3;

public class ConflictingStubTests
{
    [Fact]
    public void CanStubWithConflictingTypes()
    {
        var stub = new Confliting1Stub();

        Assert.NotNull(stub.ConflictingName1);
        Assert.False(((IConflictingInterface1)stub).ConflictingName1);
    }

    [Fact]
    public void CanStubWithConflictingAccessors()
    {
        var stub = new Confliting2Stub();

        Assert.NotNull(stub.ConflictingName1);
        ((IConflictingInterface2)stub).ConflictingName1 = "";
    }

    [Fact]
    public void CanStubWithConflictingMembers()
    {
        var stub = new Confliting3Stub();

        Assert.NotNull(stub.ConflictingName1);
        Assert.False(((IConflictingInterface3)stub).ConflictingName1());
    }

    [Fact]
    public void CanStubVariousConflicts()
    {
        var stub = new ConflitingAllStub();

        Assert.NotNull(stub.ConflictingName1);
        Assert.False(((IConflictingInterface1)stub).ConflictingName1);
        ((IConflictingInterface2)stub).ConflictingName1 = "";
        Assert.False(((IConflictingInterface3)stub).ConflictingName1());
    }
}