using System.Diagnostics.CodeAnalysis;

namespace Nubbin.UsageTest.Bases;

public abstract class AnnotationClassSample
{
    [NotNull]
    public abstract DefaultConstructible? Property { get; set; }
    [NotNullIfNotNull(nameof(Property))]
    public abstract DefaultConstructible? DependentProperty { get; }

    public abstract void Method([NotNull] object? value);
    public abstract bool ConditionalMethod([NotNullWhen(true)] object? value);
    public abstract bool ConditionalMethodInverse([NotNullWhen(false)] object? value);
}

public interface IAnnotationInterfaceSample
{
    [NotNull]
    DefaultConstructible? Property { get; set; }
    [NotNullIfNotNull(nameof(Property))]
    DefaultConstructible? DependentProperty { get; }

    void Method([NotNull] object? value);
    bool ConditionalMethod([NotNullWhen(true)] object? value);
    bool ConditionalMethodInverse([NotNullWhen(false)] object? value);
}
