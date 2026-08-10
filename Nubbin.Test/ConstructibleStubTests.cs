using Nubbin.Test.Samples;

namespace Nubbin.Test;

public class ConstructibleStubTests
{
    [Fact]
    public void StubbedConstructibleReturnUsesNull()
    {
        var result = new ConstructibleStub().Create();

        Assert.Null(result);
    }

    [Fact]
    public async Task StubbedTaskUsesNullResult()
    {
        var result = await new ConstructibleStub().CreateAsync();

        Assert.Null(result);
    }
}