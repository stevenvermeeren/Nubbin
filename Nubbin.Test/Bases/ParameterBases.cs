namespace Nubbin.Test.Bases;

public interface IParameterSample
{
    void Update(ref int value);
    void Read(out int value);
    void ReadNullable(out string? value);
    void ReadConstructible(out DefaultConstructible value);
    void ReadUnsupported(out NoDefaultConstructor value);
    void Inspect(in int value);
}
