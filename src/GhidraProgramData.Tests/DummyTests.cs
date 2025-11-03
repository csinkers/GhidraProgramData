using FluentAssertions;
using GhidraProgramData.Types;

namespace GhidraProgramData.Tests;

public class DummyTests
{
    [Fact]
    public void UnknownTypeNameReturnsDummy()
    {
        var pd = new TestXmlBuilder().Load();
        var t = pd.Types[new TypeKey("/", "nonexistent")];
        t.Should().NotBeNull();
        t.Should().BeOfType<GDummy>();
        var st = (GDummy)t;
        st.FixedSize.Should().Be(0);
        st.Key.Name.Should().Be("nonexistent");
    }
}