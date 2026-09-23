using Nubbin.UsageTest.Bases;

namespace Nubbin.UsageTest;

[Stub]
public partial class DelegateStub : IDelegateSample
{
}

public class DelegateStubTests
{
    [Fact]
    public void ReturnsExecutableAction()
    {
        var result = new DelegateStub().GetAction();

        Assert.NotNull(result);
        var ex = Record.Exception(result);
        Assert.Null(ex);
    }

    [Fact]
    public void ReturnsExecutableActionWithParam()
    {
        var result = new DelegateStub().GetActionWithParam();

        Assert.NotNull(result);
        var ex = Record.Exception(() => result("input", 0));
        Assert.Null(ex);
    }

    [Fact]
    public void ReturnsFuncWithResult()
    {
        var result = new DelegateStub().GetConstructibleFunc();

        Assert.NotNull(result);
        var res = result();
        Assert.Equal(42, res.Value);
    }
    
    [Fact]
    public void ReturnsFuncWithException()
    {
        var result = new DelegateStub().GetUnconstructibleFunc();

        Assert.NotNull(result);
        var ex = Record.Exception(result);
        Assert.NotNull(ex);
    }
    
    [Fact]
    public void ReturnsFuncWithParam()
    {
        var result = new DelegateStub().GetFuncWithParam();

        Assert.NotNull(result);
        var res = result("input", 0);
        Assert.Equal(42, res.Value);
    }

    [Fact]
    public void ReturnsPredicate()
    {
        var result = new DelegateStub().GetPredicate();

        Assert.NotNull(result);
        var res = result(0);
        Assert.False(res);
    }
    
    [Fact]
    public void ReturnsDelegate()
    {
        var result = new DelegateStub().GetDelegate();

        Assert.NotNull(result);
        var res = result("input", 0);
        Assert.Equal(42, res.Value);
    }
}