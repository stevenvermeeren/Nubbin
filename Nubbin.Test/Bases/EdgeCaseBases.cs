namespace Nubbin.Test.Bases;

public interface IUnsupportedReferenceSample
{
    NoDefaultConstructor Missing();
}

public sealed class NoDefaultConstructor
{
    public NoDefaultConstructor(int value)
    {
    }
}

#nullable disable

public interface IObliviousReferenceSample
{
    string Missing();
}

public interface IObliviousOutParameterSample
{
    void Read(out string value);
}

#nullable restore
