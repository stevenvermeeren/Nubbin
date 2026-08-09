using Nubbin.Test.Samples;

namespace Nubbin.Test;

public class ConstructibleStubTests
{
    [Fact]
    public void StubbedConstructibleReturnUsesDefaultConstructor()
    {
        var result = new ConstructibleStub().Create();

        Assert.NotNull(result);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task StubbedTaskUsesDefaultConstructorForResult()
    {
        var result = await new ConstructibleStub().CreateAsync();

        Assert.NotNull(result);
        Assert.Equal(42, result.Value);
    }
}