using Microsoft.CodeAnalysis;
using Nubbin.Analyzer.Emitting;

namespace Nubbin.Test.Generator.Emitting;

public class SymbolFormattingTests
{
    [Fact]
    public void MapsAccessibilityToCSharpKeyword()
    {
        Assert.Equal("public", Accessibility.Public.AsTypeAccessibility());
        Assert.Equal("internal", Accessibility.Internal.AsTypeAccessibility());
        Assert.Equal("private", Accessibility.Private.AsTypeAccessibility());
        Assert.Equal(string.Empty, Accessibility.ProtectedAndInternal.AsTypeAccessibility());
    }

    [Fact]
    public void GetsMemberAccessibilityForInterfaceAndProtectedMembers()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            public interface IContract
            {
                int Value { get; }
            }

            public class Base
            {
                public int PublicValue => 0;
                protected int ProtectedValue => 0;
                protected internal int ProtectedInternalValue => 0;
                private protected int PrivateProtectedValue => 0;
            }
            """);

        var assembly = compilation.Assembly;

        Assert.Equal(
            "public",
            GeneratorTestHelpers.GetType(compilation, "IContract").GetMembers("Value").OfType<IPropertySymbol>().Single().GetMemberAccessibility(assembly));
        Assert.Equal(
            "public",
            GeneratorTestHelpers.GetType(compilation, "Base").GetMembers("PublicValue").OfType<IPropertySymbol>().Single().GetMemberAccessibility(assembly));
        Assert.Equal(
            "protected",
            GeneratorTestHelpers.GetType(compilation, "Base").GetMembers("ProtectedValue").OfType<IPropertySymbol>().Single().GetMemberAccessibility(assembly));
        Assert.Equal(
            "protected internal",
            GeneratorTestHelpers.GetType(compilation, "Base").GetMembers("ProtectedInternalValue").OfType<IPropertySymbol>().Single().GetMemberAccessibility(assembly));
        Assert.Equal(
            "private protected",
            GeneratorTestHelpers.GetType(compilation, "Base").GetMembers("PrivateProtectedValue").OfType<IPropertySymbol>().Single().GetMemberAccessibility(assembly));
    }

    [Fact]
    public void FormatsNestedGenericNamesAndTypeParameters()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            namespace Example;
            public class Outer<T>
            {
                public class Inner<U>
                {
                    public T Value { get; }
                }
            }
            """);

        var nestedType = GeneratorTestHelpers.GetType(compilation, "Example.Outer`1+Inner`1");
        var genericType = GeneratorTestHelpers.GetType(compilation, "Example.Outer`1");

        Assert.Contains("Example.Outer.Inner", nestedType.GetFullyQualifiedName(false));
        Assert.Contains("<T>", genericType.FormatTypeParams());
        Assert.Contains("Inner", nestedType.GetStubTypeName());
    }

    [Fact]
    public void FormatsAttributeMetadataAndTypeConstraints()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            using System;
            using System.Diagnostics.CodeAnalysis;

            public class Subject<T, U>
                where T : class, IDisposable
                where U : notnull, new()
            {
                [NotNull]
                public T Value { get; set; }
            }
            """);

        var type = GeneratorTestHelpers.GetType(compilation, "Subject`2");
        var property = type.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var builder = new IndentedStringBuilder();

        builder.AppendTypeConstraints(type.TypeParameters);
        var result = builder.ToString();

        Assert.Contains("where T : IDisposable", result);
        Assert.Contains("where U : notnull, new()", result);
        Assert.DoesNotContain("NullableAttribute", property.GetAttributes().Format());
        Assert.Contains("NotNullAttribute", property.GetAttributes().Format());
    }

    [Fact]
    public void CoversRemainingFormattingEdgeCases()
    {
        var compilation = GeneratorTestHelpers.CreateCompilation(
            """
            using System;
            using System.Diagnostics.CodeAnalysis;

            public class Subject<T, U, V>
                where T : class
                where U : notnull, new()
                where V : struct
            {
                public string? NullableValue { get; set; }
                public T Value { get; set; }
                public U FactoryValue { get; set; }
                public V NumberValue { get; set; }
                private int PrivateValue => 0;
                internal int InternalValue => 0;
                protected int ProtectedValue => 0;
                protected internal int ProtectedInternalValue => 0;
            }

            public class Empty<T>
            {
                public T Value { get; set; }
            }

            public class Holder
            {
                public System.Collections.Generic.List<int> Values { get; set; }
            }

            namespace Sample
            {
                public class Token { }
            }

            public class TokenHolder
            {
                public System.Collections.Generic.List<Sample.Token> Value { get; set; }
            }

            namespace System.Runtime.CompilerServices
            {
                public class NullableAttribute : Attribute
                {
                    public NullableAttribute(int value) { }
                }
            }

            public class NullableAnnotated
            {
                [System.Runtime.CompilerServices.Nullable(1)]
                [System.Diagnostics.CodeAnalysis.NotNull]
                public string Value { get; set; }
            }
            """);

        var subjectType = GeneratorTestHelpers.GetType(compilation, "Subject`3");
        var emptyType = GeneratorTestHelpers.GetType(compilation, "Empty`1");
        var holderType = GeneratorTestHelpers.GetType(compilation, "Holder");
        var tokenHolderType = GeneratorTestHelpers.GetType(compilation, "TokenHolder");
        var typeParam = emptyType.TypeParameters[0];
        var nullableProperty = subjectType.GetMembers("NullableValue").OfType<IPropertySymbol>().Single();
        var privateProperty = subjectType.GetMembers("PrivateValue").OfType<IPropertySymbol>().Single();
        var protectedInternalProperty = subjectType.GetMembers("ProtectedInternalValue").OfType<IPropertySymbol>().Single();
        var internalProperty = subjectType.GetMembers("InternalValue").OfType<IPropertySymbol>().Single();
        var annotatedProperty = GeneratorTestHelpers.GetType(compilation, "NullableAnnotated").GetMembers("Value").OfType<IPropertySymbol>().Single();

        Assert.Equal(string.Empty, ((INamedTypeSymbol?)null).FormatTypeParams());
        Assert.Equal(string.Empty, typeParam.FormatConstraints());
        var valuesType = (INamedTypeSymbol)holderType.GetMembers("Values").OfType<IPropertySymbol>().Single().Type;
        var tokenValueType = (INamedTypeSymbol)tokenHolderType.GetMembers("Value").OfType<IPropertySymbol>().Single().Type;
        Console.WriteLine(valuesType.FormatTypeParams());
        Console.WriteLine(tokenValueType.FormatTypeParams());
        Assert.Contains("int", valuesType.FormatTypeParams());
        Assert.Contains("Sample.Token", tokenValueType.FormatTypeParams());
        Assert.Equal("public", privateProperty.GetMemberAccessibility(compilation.Assembly));
        Assert.Equal("internal", internalProperty.GetMemberAccessibility(compilation.Assembly));
        Assert.Equal("protected", subjectType.GetMembers("ProtectedValue").OfType<IPropertySymbol>().Single().GetMemberAccessibility(compilation.Assembly));
        Assert.Equal("protected internal", protectedInternalProperty.GetMemberAccessibility(compilation.Assembly));
        Assert.DoesNotContain("NullableAttribute", nullableProperty.GetAttributes().Format());
        Assert.DoesNotContain("NullableAttribute", annotatedProperty.GetAttributes().Format());
        Assert.Contains("string?", nullableProperty.Type.ToQualifiedString());
        Assert.Contains("where T : class", subjectType.TypeParameters[0].FormatConstraints());
        Assert.Contains("where U : notnull, new()", subjectType.TypeParameters[1].FormatConstraints());
        Assert.Contains("where V : struct", subjectType.TypeParameters[2].FormatConstraints());

        var emptyBuilder = new IndentedStringBuilder();
        emptyBuilder.AppendTypeConstraints(new[] { emptyType.TypeParameters[0], subjectType.TypeParameters[0] });
        Assert.Contains("where T : class", emptyBuilder.ToString());
        Assert.DoesNotContain("where U :", emptyBuilder.ToString());

        var otherCompilation = GeneratorTestHelpers.CreateCompilation(
            "public class Other { protected internal int Value => 0; }");
        var otherProperty = GeneratorTestHelpers.GetType(otherCompilation, "Other").GetMembers("Value").OfType<IPropertySymbol>().Single();
        Assert.Equal("protected", otherProperty.GetMemberAccessibility(compilation.Assembly));
    }
}
