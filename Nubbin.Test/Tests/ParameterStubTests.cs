using Nubbin;
using Nubbin.Test.Bases;

namespace Nubbin.Test;

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
    public void OutParametersUseReturnValueDefaults()
    {
        var stub = new ParameterStub();

        stub.ReadNullable(out var nullableResult);
        stub.ReadConstructible(out var constructibleResult);

        Assert.Null(nullableResult);
        Assert.Equal(42, constructibleResult.Value);
    }

    [Fact]
    public void UnsupportedNonNullableOutParameterThrows()
    {
        Assert.Throws<NotImplementedException>(() => new ParameterStub().ReadUnsupported(out _));
    }
}