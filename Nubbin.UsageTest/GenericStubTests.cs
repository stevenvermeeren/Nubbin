using Nubbin.UsageTest.Bases;

namespace Nubbin.UsageTest;

[Stub] public partial class GenericStub<T> : GenericSample<T> { }
[Stub] public partial class ConstrainedGenericStub<T> : ConstrainedGenericSample<T> where T : IComparable, new() { }
[Stub] public partial class GenericMembersStub : GenericMembersSample { }
[Stub] public partial class ContravariantStub<T> : IContravariantSample<T> { }
[Stub] public partial class CovariantStub<T> : ICovariantSample<T> where T : struct { } 

public class StubDefaultsTests
{
    [Fact]
    public void CanStubGenericType()
    {
        var stub = new GenericStub<string>();

        var ex = Record.Exception(() => stub.Property);
        Assert.IsType<NotImplementedException>(ex);
        var ex2 = Record.Exception(() => stub.Method("param"));
        Assert.Null(ex2);
    }

    [Fact]
    public void CanStubConstrainedGenericType()
    {
        var stub = new ConstrainedGenericStub<double>();

        Assert.Equal(0.0, stub.Property);
        var ex = Record.Exception(() => stub.Method(0));
        Assert.Null(ex);
    }

    [Fact]
    public void CanStubGenericMembers()
    {
        var stub = new GenericMembersStub();

        var ex = Record.Exception(() => stub.GenericMethod("param"));
        Assert.Null(ex);
        var ex2 = Record.Exception(() => stub.ConstrainedGenericMethod("param"));
        Assert.Null(ex2);
    }

    [Fact]
    public void CanStubVariantInterface()
    {
        var stub = new ContravariantStub<string>();

        var ex = Record.Exception(() => stub.Method("param"));
        Assert.Null(ex);
    }

    [Fact]
    public void CanStubCovariantInterface()
    {
        var stub = new CovariantStub<int>();

        var res = stub.Method();
        Assert.Equal(0, res);
    }

    [Fact]
    public void AutoStub_UsesFullyQualifiedTypeArguments()
    {
        var stub = Stub.Auto<GenericStub<TypeArg>>();

        var ex = Record.Exception(() => stub.Property);
        Assert.IsType<NotImplementedException>(ex);
        var ex2 = Record.Exception(() => stub.Method(new TypeArg(0)));
        Assert.Null(ex2);
    }

    internal record TypeArg(int _);
}