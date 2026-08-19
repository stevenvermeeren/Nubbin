using Nubbin.UsageTests.Bases;

namespace Nubbin.UsageTests;

[Stub]
public partial class ConstructibleStub : IConstructibleSample
{
}

public class ConstructibleStubTests
{
    [Fact]
    public void StubbedConstructibleReturnUsesDefaultConstructor()
    {
        var result = new ConstructibleStub().Create();

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task StubbedTaskUsesDefaultConstructorForResult()
    {
        var result = await new ConstructibleStub().CreateAsync();

        Assert.Equal(42, result.Value);
    }
}