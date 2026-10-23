using Microsoft.CodeAnalysis;
using Nubbin.Analyzer;
using Nubbin.Analyzer.Discovery;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class PropertyEmitterTests
{
    private static MissingProperty Missing(IPropertySymbol property, StubDefinition type) => new(property, type);

    [Fact]
    public void AppendPropertyUsesBackingStorageWhenRequired()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string Name { get; } }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("public override string Name", result);
        Assert.Contains("get => __Nubbin__PropertyContainer.Name;", result);
    }

    [Fact]
    public void AppendAutoPropertyBodyGeneratesAutomaticPropertyForValueType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public int Count { get; set; } }");
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendAutoPropertyBody(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("{ get; set; } = default;", result);
    }

    [Fact]
    public void AppendProperty_UseStorage_WritesSetterOnly()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string Name { set; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("set => __Nubbin__PropertyContainer.Name = value;", result);
        Assert.DoesNotContain("get => __Nubbin__PropertyContainer.Name;", result);
    }

    [Fact]
    public void AppendProperty_UsesBackingStorageForGetterOnly()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public string Name { get; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("get => __Nubbin__PropertyContainer.Name;", result);
        Assert.DoesNotContain("set => __Nubbin__PropertyContainer.Name = value;", result);
    }

    [Fact]
    public void AppendProperty_UsesPropertyStorageWhenBothAccessorsExist()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject { public int Count { get; set; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("get => __Nubbin__PropertyContainer.Count;", result);
        Assert.Contains("set => __Nubbin__PropertyContainer.Count = value;", result);
        Assert.DoesNotContain("{ get; set; } = default;", result);
    }

    [Fact]
    public void AppendProperty_OneSidedInterfacePropertyExposesBothAccessors()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public interface ISubject { string Name { get; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.ISubject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("get => __Nubbin__PropertyContainer.Name;", result);
        Assert.Contains("set => __Nubbin__PropertyContainer.Name = value;", result);
    }

    [Fact]
    public void AppendProperty_UsesPropertyStorageForNonConstructibleReferenceType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject(int param) { public Subject Name { get; set; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("get => __Nubbin__PropertyContainer.Name;", result);
        Assert.Contains("set => __Nubbin__PropertyContainer.Name = value;", result);
        Assert.DoesNotContain("private global::Example.Subject? __Nubbin__Name;", result);
    }

    [Fact]
    public void AppendAutoPropertyBody_UsesNullableBackingFieldForNonConstructibleGenericType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            "namespace Example; public class Subject<T> { public T Name { get; set; } }"
        );
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject`1"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendAutoPropertyBody(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("get => __Nubbin__Name ?? throw new global::System.NotImplementedException();", result);
        Assert.Contains("set => __Nubbin__Name = value;", result);
    }

    [Fact]
    public void AppendProperty_UsesUniqueBackingFieldNamesForExplicitMembersWithSameName()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public class Subject(int param) { }
            public interface IFirst { Subject Name { get; } }
            public interface ISecond { string Name { get; } }
            """);

        var firstType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IFirst"));
        var secondType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.ISecond"));
        var firstProperty = firstType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();
        var secondProperty = secondType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        var first = Missing(firstProperty, firstType);
        var second = Missing(secondProperty, secondType);
        first.Process([first, second]);

        Assert.True(first.ImplementExplicitly);
        Assert.False(second.ImplementExplicitly);

        builder.AppendProperty(first);

        var result = builder.ToString();

        Assert.Contains("get => throw new global::System.NotImplementedException();", result);
        Assert.DoesNotContain("private global::Example.Subject? __Nubbin__global__Example_IFirst_Name;", result);
        Assert.DoesNotContain("private global::System.String? __Nubbin__Name;", result);
        Assert.DoesNotContain("__Nubbin__Name;", result);
    }

    [Fact]
    public void AppendAutoPropertyBody_GeneratesSetterOnlyExplicitProperty()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public interface IFirst { int Name { set; } }
            public interface ISecond { int Name { get; } }
            """);
        var firstType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IFirst"));
        var secondType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.ISecond"));
        var firstProperty = firstType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();
        var secondProperty = secondType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        var missing = Missing(firstProperty, firstType);
        var other = Missing(secondProperty, secondType);
        missing.Process([missing, other]);
        builder.AppendAutoPropertyBody(missing);

        var result = builder.ToString();

        Assert.Contains("set { }", result);
        Assert.DoesNotContain("get =>", result);
        Assert.DoesNotContain("private ", result);
        Assert.DoesNotContain("__Nubbin__", result);
    }

    [Fact]
    public void AppendAutoPropertyBody_GeneratesGetterOnlyExplicitProperty()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public interface IFirst { int Name { get; } }
            public interface ISecond { int Name { set; } }
            """);
        var firstType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IFirst"));
        var secondType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.ISecond"));
        var firstProperty = firstType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();
        var secondProperty = secondType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        var missing = Missing(firstProperty, firstType);
        var other = Missing(secondProperty, secondType);
        missing.Process([missing, other]);
        builder.AppendAutoPropertyBody(missing);

        var result = builder.ToString();

        Assert.Contains("get => default;", result);
        Assert.DoesNotContain("set", result);
    }

    [Fact]
    public void AppendProperty_UsesPropertyStorageForAbstractBaseSetterOnlyProperty()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public abstract class Base
            {
                public abstract int Value { set; }
            }
            """);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Base"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("set => __Nubbin__PropertyContainer.Value = value;", result);
        Assert.DoesNotContain("get => __Nubbin__PropertyContainer.Value;", result);
        Assert.DoesNotContain("private int __Nubbin__Value;", result);
    }

    [Fact]
    public void AppendAutoPropertyBody_GeneratesNoOpSetterForExplicitSetterOnlyNonConstructibleType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public class ValueTypeHolder(int value) { }
            public interface IFirst { ValueTypeHolder Name { set; } }
            public interface ISecond { ValueTypeHolder Name { get; } }
            """);
        var firstType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IFirst"));
        var secondType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.ISecond"));
        var firstProperty = firstType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();
        var secondProperty = secondType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        var missing = Missing(firstProperty, firstType);
        var other = Missing(secondProperty, secondType);
        missing.Process([missing, other]);
        builder.AppendAutoPropertyBody(missing);

        var result = builder.ToString();

        Assert.Contains("set { }", result);
        Assert.DoesNotContain("get =>", result);
        Assert.DoesNotContain("private ", result);
        Assert.DoesNotContain("__Nubbin__", result);
    }

    [Fact]
    public void AppendProperty_DoesNotGenerateBackingFieldWhenUsingPropertyStorage()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public class Subject(int param) { }
            public interface ISubject { Subject Name { get; } }
            """);
        var type = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.ISubject"));
        var property = type.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        builder.AppendProperty(Missing(property, type));

        var result = builder.ToString();

        Assert.Contains("get => __Nubbin__PropertyContainer.Name;", result);
        Assert.DoesNotContain("private global::Example.Subject? __Nubbin__Name;", result);
    }

    [Fact]
    public void AppendAutoPropertyBody_GeneratesNotImplementedForExplicitGetterOnlyNonConstructibleType()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public class ValueTypeHolder(int value) { }
            public interface IFirst { ValueTypeHolder Name { get; } }
            public interface ISecond { ValueTypeHolder Name { set; } }
            """);
        var firstType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.IFirst"));
        var secondType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.ISecond"));
        var firstProperty = firstType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();
        var secondProperty = secondType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        var missing = Missing(firstProperty, firstType);
        var other = Missing(secondProperty, secondType);
        missing.Process([missing, other]);
        builder.AppendAutoPropertyBody(missing);

        var result = builder.ToString();

        Assert.Contains("throw new global::System.NotImplementedException();", result);
    }

    [Fact]
    public void AppendAutoPropertyBody_GeneratesPartialExplicitlyImplementedProperty()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public interface Subject<T> { T Name { get; } }
            public interface Other { int Name { get; } }
            """);
        var subjectType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Subject`1"));
        var otherType = StubDefinition.FromINamedTypeSymbol(GeneratorTestHelpers.GetType(compilation, "Example.Other"));
        var property = subjectType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();
        var conflicting = otherType.LeafType!.GetMembers().OfType<IPropertySymbol>().Single();

        var builder = new IndentedStringBuilder();
        var missing = Missing(property, subjectType);
        var other = Missing(conflicting, otherType);
        missing.Process([missing, other]);
        builder.AppendAutoPropertyBody(missing);

        var result = builder.ToString();

        Assert.DoesNotContain("set", result);
        Assert.Contains("get => throw new global::System.NotImplementedException();", result);
        Assert.DoesNotContain("__Nubbin__", result);
    }
}
