using Nubbin.UsageTest.Bases;

namespace Nubbin.UsageTest;

public class AnnotatedStubTests
{
    [Fact]
    public void CanStubClassWithNullabilityAnnotations()
    {
        var classStub = Stub.Auto<AnnotationClassSample>();

        Assert.NotNull(classStub.Property);
        Assert.NotNull(classStub.DependentProperty);

        var ex = Record.Exception(() => classStub.Method(null));
        Assert.IsType<NotImplementedException>(ex);

        Assert.False(classStub.ConditionalMethod(null));

        var ex2 = Record.Exception(() => classStub.ConditionalMethodInverse(null));
        Assert.IsType<NotImplementedException>(ex2);
    }

    [Fact]
    public void CanStubInterfaceWithNullabilityAnnotations()
    {
        var interfaceStub = Stub.Auto<IAnnotationInterfaceSample>();

        Assert.NotNull(interfaceStub.Property);
        Assert.NotNull(interfaceStub.DependentProperty);

        var ex = Record.Exception(() => interfaceStub.Method(null));
        Assert.IsType<NotImplementedException>(ex);

        Assert.False(interfaceStub.ConditionalMethod(null));

        var ex2 = Record.Exception(() => interfaceStub.ConditionalMethodInverse(null));
        Assert.IsType<NotImplementedException>(ex2);
    }
}