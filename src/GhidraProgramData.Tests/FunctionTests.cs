using FluentAssertions;
using GhidraProgramData.Types;

namespace GhidraProgramData.Tests;

public class FunctionTests
{
    [Fact]
    public void VoidVoidFunction()
    {
        ProgramData pd = new TestXmlBuilder().Type(@"
        <FUNCTION_DEF NAME=""VoidVoidFunc"" NAMESPACE=""/Foo"">
            <REGULAR_CMT>Function Signature Data Type</REGULAR_CMT>
            <RETURN_TYPE DATATYPE=""void"" DATATYPE_NAMESPACE=""/"" SIZE=""0x0"" />
        </FUNCTION_DEF>
").Load();

        var t = pd.Types["Foo/VoidVoidFunc"];
        t.Should().NotBeNull();
        t.Should().BeOfType<GFuncPointer>();
        var st = (GFuncPointer)t;
        st.FixedSize.Should().Be(4);
        st.ReturnType.Should().Be(pd.Types["void"]);
        st.Parameters.Should().NotBeNull();
        st.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void IntIntFunction()
    {
        ProgramData pd = new TestXmlBuilder().Type(@"
        <FUNCTION_DEF NAME=""IntIntFunc"" NAMESPACE=""/Foo"">
            <REGULAR_CMT>Function Signature Data Type</REGULAR_CMT>
            <RETURN_TYPE DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" SIZE=""0x0"" />
            <PARAMETER ORDINAL=""0x0"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""p1"" SIZE=""0x4"" />
        </FUNCTION_DEF>
").Load();

        var t = pd.Types["Foo/IntIntFunc"];
        t.Should().NotBeNull();
        t.Should().BeOfType<GFuncPointer>();

        var st = (GFuncPointer)t;
        st.FixedSize.Should().Be(4);
        st.ReturnType.Should().Be(pd.Types["int"]);
        st.Parameters.Should().NotBeNull();
        st.Parameters.Should().HaveCount(1);

        var p1 = st.Parameters[0];
        p1.Name.Should().Be("p1");
        p1.Ordinal.Should().Be(0);
        p1.Size.Should().Be(4);
        p1.Type.Should().Be(pd.Types["int"]);
    }
}