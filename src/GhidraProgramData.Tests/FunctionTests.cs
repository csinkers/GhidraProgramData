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
}