namespace Nubbin.Test.UsageTests.Bases;

public interface IAsyncSample
{
    Task RunAsync();
    Task<string?> GetAsync();
}
