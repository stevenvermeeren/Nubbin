namespace Nubbin.UsageTests.Bases;

public interface IAsyncSample
{
    Task RunAsync();
    Task<string?> GetAsync();
}
