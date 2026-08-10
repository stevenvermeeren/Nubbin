using Nubbin;
using Nubbin.Test.Bases;

namespace Nubbin.Test;

[Stub]
public partial class AsyncStub : IAsyncSample
{
}

public class AsyncStubTests
{
    [Fact]
    public async Task StubbedTaskMethodReturnsCompletedTask()
    {
        var task = new AsyncStub().RunAsync();

        Assert.NotNull(task);
        Assert.True(task.IsCompletedSuccessfully);
        await task;
    }

    [Fact]
    public async Task StubbedGenericTaskMethodReturnsDefaultResult()
    {
        var result = await new AsyncStub().GetAsync();

        Assert.Null(result);
    }
}