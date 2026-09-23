namespace Nubbin.UsageTest.Bases;

public delegate DefaultConstructible SampleDelegate(string text, int param);
public interface IDelegateSample
{
    Action GetAction();
    Action<string, int> GetActionWithParam();
    Func<DefaultConstructible> GetConstructibleFunc();
    Func<IComparable> GetUnconstructibleFunc();
    Func<string, int, DefaultConstructible> GetFuncWithParam();
    Predicate<int> GetPredicate();
    SampleDelegate GetDelegate();
}