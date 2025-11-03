using FluentAssertions;
using GhidraProgramData.Types;

namespace GhidraProgramData.Tests;

public class EnumTests
{
    [Fact]
    public void SimpleEnumTest()
    {
        ProgramData pd = new TestXmlBuilder()
            .Type(@"
<ENUM NAME=""SimpleEnum"" NAMESPACE=""/foo"" SIZE=""0x2"">
    <ENUM_ENTRY NAME=""Unknown"" VALUE=""0x0"" COMMENT="""" />
    <ENUM_ENTRY NAME=""First"" VALUE=""0x1"" COMMENT=""Comments are currently ignored, but shouldn't cause errors"" />
    <ENUM_ENTRY NAME=""Second"" VALUE=""0x2"" COMMENT="""" />
    <ENUM_ENTRY NAME=""Skipped"" VALUE=""0x4"" COMMENT="""" />
</ENUM>
")
            .Load();


        pd.Should().NotBeNull();
        pd.Types.Should().NotBeNull();
        var type = pd.Types[new TypeKey("/foo", "SimpleEnum")];
        type.Should().BeOfType<GEnum>();
        type.FixedSize.Should().Be(2);

        var staticType = (GEnum)type;
        staticType.Size.Should().Be(2);
        staticType.Elements.Count.Should().Be(4);
        staticType.Elements.Keys.OrderBy(x => x).Should().BeEquivalentTo(new[] { 0, 1, 2, 4 });
        staticType.Elements[0].Should().Be("Unknown");
        staticType.Elements[1].Should().Be("First");
        staticType.Elements[2].Should().Be("Second");
        staticType.Elements[4].Should().Be("Skipped");
    }
}