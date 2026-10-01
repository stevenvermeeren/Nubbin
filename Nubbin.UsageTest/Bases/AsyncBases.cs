namespace Nubbin.UsageTest.Bases;

public interface IAsyncSample
{
    Task RunAsync();
    Task<string?> GetAsync();
    Task<string?> TaskProperty { get; set; }
}
