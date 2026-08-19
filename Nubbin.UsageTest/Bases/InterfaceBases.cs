namespace Nubbin.UsageTests.Bases;

public interface InterfaceSample
{
    int Compare(int left, int right);
    string? Name { get; set; }
    object? GetterOnly { get; }
    int SetterOnly { set; }
    IEnumerable<string> Items();
    IList<int> Numbers { get; set; }
    IDictionary<string, int> Counts();
    ISet<string> Tags { get; set; }
}

public interface IPartialInterfaceSample
{
    int Existing();
    string? Missing();
}

public interface IFullyImplementedInterfaceSample
{
    int Existing();
    string? Missing();
}

public interface IHybridSample
{
    bool IsValid(string value);
}

public abstract class HybridBase
{
    public abstract string? Create(int value);
    public abstract bool GetterOnly { get; }
}
