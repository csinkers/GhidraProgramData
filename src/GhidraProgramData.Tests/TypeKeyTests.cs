using FluentAssertions;

namespace GhidraProgramData.Tests;

public class TypeKeyTests
{
    static void ParseTest(string s, string expectedNamespace, string expectedName)
    {
        var k = TypeKey.Parse(s);
        k.Should().NotBeNull();
        k.Namespace.Should().Be(expectedNamespace);
        k.Name.Should().Be(expectedName);

        var asString = k.ToString();
        var reparsed = TypeKey.Parse(asString);
        reparsed.Namespace.Should().Be(expectedNamespace);
        reparsed.Name.Should().Be(expectedName);
    }

    [Fact] public void ParsePrimitive() => ParseTest("int", "", "int");
    [Fact] public void ParsePrimitiveWithLeadingSlash() => ParseTest("/int", "", "int");
    [Fact] public void ParseWithNamespaceAndLeadingSlash() => ParseTest("/foo/bar", "foo", "bar");
    [Fact] public void ParseWithNestedNamespaceAndLeadingSlash() => ParseTest("/foo/bar/baz", "foo/bar", "baz");
    [Fact] public void ParseWithNamespaceAndNoLeadingSlash() => ParseTest("foo/bar", "foo", "bar");
    [Fact] public void ParseWithNestedNamespaceAndNoLeadingSlash() => ParseTest("foo/bar/baz", "foo/bar", "baz");
}