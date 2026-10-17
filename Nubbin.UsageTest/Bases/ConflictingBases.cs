namespace Nubbin.UsageTest.Bases;

public abstract class ConflictingBase
{
    public abstract string ConflictingName1 { get; }
    public abstract event EventHandler ConflictingName2;
    public abstract bool ConflictingName3();
    public abstract bool ConflictingName3(string param);
    public abstract bool ConflictingName3<T>() where T : class;
}

public interface IConflictingInterface1
{
    bool ConflictingName1 { get; }
    event EventHandler<string> ConflictingName2;
    string ConflictingName3();
    bool ConflictingName3(bool param);
    bool ConflictingName3<T>() where T : struct;
}
public interface IConflictingInterface2
{
    string ConflictingName1 { set; }
}
public interface IConflictingInterface3
{
    bool ConflictingName1();
    bool ConflictingName2 { get; set; }
    event EventHandler ConflictingName3;
}