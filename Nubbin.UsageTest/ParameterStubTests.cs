using Nubbin.UsageTests.Bases;

namespace Nubbin.UsageTest;

[Stub]
public partial class ParameterStub : IParameterSample
{
}

public class ParameterStubTests
{
    [Fact]
    public void RefInAndOutParametersCompileAndUseDefaultValues()
    {
        var stub = new ParameterStub();
        var value = 1;

        stub.Update(ref value);
        stub.Inspect(value);
        stub.Read(out var result);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task OutParametersUseReturnValueDefaults()
    {
        var stub = new ParameterStub();

        stub.ReadNullable(out var nullableResult);
        stub.ReadConstructible(out var constructibleResult);
        stub.ReadTask(out var basicTask);
        stub.ReadConstructibleTask(out var typedTask);

        Assert.Null(nullableResult);
        Assert.Equal(42, constructibleResult.Value);
        Assert.True(basicTask.IsCompleted);
        Assert.Equal(42, (await typedTask).Value);
    }

    [Fact]
    public void UnsupportedNonNullableOutParameterThrows()
    {
        Assert.Throws<NotImplementedException>(() => new ParameterStub().ReadUnsupported(out _));
        Assert.Throws<NotImplementedException>(() => new ParameterStub().ReadUnsupportedTask(out _));
    }
}