namespace Nubbin.Test.UsageTests.Bases;

public interface IParameterSample
{
    void Update(ref int value);
    void Read(out int value);
    void ReadNullable(out string? value);
    void ReadConstructible(out DefaultConstructible value);
    void ReadUnsupported(out NoDefaultConstructor value);
    void ReadTask(out Task value);
    void ReadConstructibleTask(out Task<DefaultConstructible> value);
    void ReadUnsupportedTask(out Task<NoDefaultConstructor> value);
    void Inspect(in int value);
}
