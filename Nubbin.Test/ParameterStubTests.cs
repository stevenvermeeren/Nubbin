using Nubbin.Test.Samples;

namespace Nubbin.Test;

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
}