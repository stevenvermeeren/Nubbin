namespace Nubbin.Test.Samples;

public interface InterfaceSample
{
    int Compare(int left, int right);
    string Name { get; set; }
    object GetterOnly { get; }
    int SetterOnly { set; }
}

[Stub]
public partial class InterfaceStub : InterfaceSample
{
}