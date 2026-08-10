namespace Nubbin.Test.Samples;

public interface InterfaceSample
{
    int Compare(int left, int right);
    string Name { get; set; }
    object GetterOnly { get; }
    int SetterOnly { set; }
    IEnumerable<string> Items();
    IList<int> Numbers { get; set; }
    IDictionary<string, int> Counts();
    ISet<string> Tags { get; set; }
}

[Stub]
public partial class InterfaceStub : InterfaceSample
{
}