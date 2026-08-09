namespace Nubbin.Test.Samples;

public interface IAsyncSample
{
    Task RunAsync();
    Task<string> GetAsync();
}

[Stub]
public partial class AsyncStub : IAsyncSample
{
}