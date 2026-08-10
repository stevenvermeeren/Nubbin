namespace Nubbin.Test.Bases;

public interface IAsyncSample
{
    Task RunAsync();
    Task<string?> GetAsync();
}
