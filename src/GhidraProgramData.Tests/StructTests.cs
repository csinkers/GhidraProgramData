using FluentAssertions;
using GhidraProgramData.Types;

namespace GhidraProgramData.Tests;

public class StructTests
{
    [Fact]
    public void SimpleStructTest()
    {
        ProgramData pd = new TestXmlBuilder()
            .Type(@"
<STRUCTURE NAME=""TwoInts"" NAMESPACE=""/Ns1/Ns2"" SIZE=""0x8"">
    <MEMBER OFFSET=""0x0"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""first"" SIZE=""0x4"" />
    <MEMBER OFFSET=""0x4"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""second"" SIZE=""0x4"" />
</STRUCTURE>")
            .Load();

        pd.Should().NotBeNull();
        pd.Types.Should().NotBeNull();
        var type = pd.Types[new TypeKey("Ns1/Ns2", "TwoInts")];
        type.Should().BeOfType<GStruct>();
        type.FixedSize.Should().Be(8);
        var staticType = (GStruct)type;
        staticType.MemberNames.Should().BeEquivalentTo("[first]", "[second]");

        var m1 = staticType.Members[0];
        m1.Name.Should().Be("first");
        m1.Offset.Should().Be(0);
        m1.Size.Should().Be(4);
        m1.Directives.Should().NotBeNull();
        m1.Directives.Should().BeEmpty();
        m1.Type.Key.Should().Be(new TypeKey("", "int"));

        var m2 = staticType.Members[1];
        m2.Name.Should().Be("second");
        m2.Offset.Should().Be(4);
        m2.Size.Should().Be(4);
        m2.Directives.Should().NotBeNull();
        m2.Directives.Should().BeEmpty();
        m2.Type.Key.Should().Be(new TypeKey("", "int"));
    }

    const string AllocTypes = @"
        <ENUM NAME=""AreaFlags"" NAMESPACE=""/Albion/Memory"" SIZE=""0x1"">
            <ENUM_ENTRY NAME=""Allocated"" VALUE=""0x1"" COMMENT="""" />
            <ENUM_ENTRY NAME=""Persistent"" VALUE=""0x2"" COMMENT="""" />
            <ENUM_ENTRY NAME=""Unk4"" VALUE=""0x4"" COMMENT="""" />
            <ENUM_ENTRY NAME=""Unk8"" VALUE=""0x8"" COMMENT="""" />
        </ENUM>
        <FUNCTION_DEF NAME=""xldInitFunc"" NAMESPACE=""/Albion/Xld"">
            <REGULAR_CMT>Function Signature Data Type</REGULAR_CMT>
            <RETURN_TYPE DATATYPE=""void"" DATATYPE_NAMESPACE=""/"" SIZE=""0x0"" />
            <PARAMETER ORDINAL=""0x0"" DATATYPE=""alloc_t *"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""pAlloc"" SIZE=""0x4"" />
            <PARAMETER ORDINAL=""0x1"" DATATYPE=""byte *"" DATATYPE_NAMESPACE=""/"" NAME=""src"" SIZE=""0x4"" />
            <PARAMETER ORDINAL=""0x2"" DATATYPE=""byte *"" DATATYPE_NAMESPACE=""/"" NAME=""dest"" SIZE=""0x4"" />
            <PARAMETER ORDINAL=""0x3"" DATATYPE=""size_t"" DATATYPE_NAMESPACE=""/stddef.h"" NAME=""size"" SIZE=""0x4"" />
        </FUNCTION_DEF>
        <STRUCTURE NAME=""alloc_node_t"" NAMESPACE=""/Albion/Memory"" SIZE=""0x12"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""void *"" DATATYPE_NAMESPACE=""/"" NAME=""pData"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x4"" DATATYPE=""dword"" DATATYPE_NAMESPACE=""/"" NAME=""memType"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x8"" DATATYPE=""alloc_node_t *"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""pNext"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0xc"" DATATYPE=""workspace_area_t *"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""pArea"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x10"" DATATYPE=""word"" DATATYPE_NAMESPACE=""/"" NAME=""workspaceNumber"" SIZE=""0x2"" />
        </STRUCTURE>
        <STRUCTURE NAME=""alloc_t"" NAMESPACE=""/Albion/Memory"" SIZE=""0x11"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""AreaFlags"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""flags"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x1"" DATATYPE=""byte"" DATATYPE_NAMESPACE=""/"" NAME=""handle"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x2"" DATATYPE=""byte"" DATATYPE_NAMESPACE=""/"" NAME=""lockCount"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x3"" DATATYPE=""byte"" DATATYPE_NAMESPACE=""/"" NAME=""length"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x4"" DATATYPE=""byte"" DATATYPE_NAMESPACE=""/"" NAME=""sizeLSB"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x5"" DATATYPE=""uint"" DATATYPE_NAMESPACE=""/"" NAME=""assetId"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x9"" DATATYPE=""alloc_type_t *"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""desc"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0xd"" DATATYPE=""alloc_node_t *"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""pHead"" SIZE=""0x4"" />
        </STRUCTURE>
        <STRUCTURE NAME=""alloc_type_t"" NAMESPACE=""/Albion/Memory"" SIZE=""0xb"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""xldInitFunc *"" DATATYPE_NAMESPACE=""/Albion/Xld"" NAME=""pfnInit"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x4"" DATATYPE=""byte"" DATATYPE_NAMESPACE=""/"" NAME=""unk4"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x5"" DATATYPE=""byte"" DATATYPE_NAMESPACE=""/"" NAME=""unk5"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x6"" DATATYPE=""byte"" DATATYPE_NAMESPACE=""/"" NAME=""flags"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x7"" DATATYPE=""char *"" DATATYPE_NAMESPACE=""/"" NAME=""description"" SIZE=""0x4"" />
        </STRUCTURE>
        <STRUCTURE NAME=""workspace_area_t"" NAMESPACE=""/Albion/Memory"" SIZE=""0x14"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""dword"" DATATYPE_NAMESPACE=""/"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x4"" DATATYPE=""uint"" DATATYPE_NAMESPACE=""/"" NAME=""sizeInBytes"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x8"" DATATYPE=""pointer"" DATATYPE_NAMESPACE=""/"" NAME=""unk8"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0xc"" DATATYPE=""workspace_area_t *"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""pNext"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x10"" DATATYPE=""alloc_t *"" DATATYPE_NAMESPACE=""/Albion/Memory"" NAME=""allocation"" SIZE=""0x4"" />
        </STRUCTURE>
";
    [Fact]
    public void AlbionAllocTest()
    {
        var pd = new TestXmlBuilder().Type(AllocTypes).Load();
        var t = pd.Types["Albion/Memory/alloc_t"];
        t.Should().BeOfType<GStruct>();
        var st = (GStruct)t;
        st.Size.Should().Be(17);
        st.FixedSize.Should().Be(17);
    }
}