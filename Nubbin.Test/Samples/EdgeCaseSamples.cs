namespace Nubbin.Test.Samples;

public interface IPartialInterfaceSample
{
    int Existing();
    string Missing();
}

[Stub]
public partial class PartialInterfaceStub : IPartialInterfaceSample
{
    public int Existing() => 42;
}

public abstract class PartialBaseSample
{
    public abstract string Missing();

    public virtual int Existing() => 42;
}

[Stub]
public partial class PartialBaseStub : PartialBaseSample
{
}

public interface IFullyImplementedInterfaceSample
{
    int Existing();
    string Missing();
}

[Stub]
public partial class FullyImplementedInterfaceStub : IFullyImplementedInterfaceSample
{
    public int Existing() => 42;

    public string Missing() => "implemented";
}

public abstract class FullyImplementedBaseSample
{
    public abstract string Missing();
}

[Stub]
public partial class FullyImplementedBaseStub : FullyImplementedBaseSample
{
    public override string Missing() => "implemented";
}

public interface ISameMemberSample
{
    int Calculate(int value);
}

public abstract class SameMemberBaseSample
{
    public abstract int Calculate(int value);
}

[Stub]
public partial class SameMemberStub : SameMemberBaseSample, ISameMemberSample
{
}

public interface IParameterSample
{
    void Update(ref int value);
    void Read(out int value);
    void Inspect(in int value);
}

[Stub]
public partial class ParameterStub : IParameterSample
{
}