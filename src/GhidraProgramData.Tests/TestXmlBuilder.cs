using System.Text;

namespace GhidraProgramData.Tests;

class TestXmlBuilder
{
    readonly List<string> _dataTypes = new();

    public TestXmlBuilder Type(string xml)
    {
        _dataTypes.Add(xml);
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"
<PROGRAM NAME=""bar.exe"" EXE_PATH=""/run/media/foo/bar.exe"" EXE_FORMAT=""Linear Executable (LE-Style DOS)"" IMAGE_BASE=""00000000"">
    <INFO_SOURCE USER=""someuser"" TOOL=""Ghidra 10.2.2"" TIMESTAMP=""Sun Jul 16 23:41:40 AEST 2023"" />
    <PROCESSOR NAME=""x86"" LANGUAGE_PROVIDER=""x86:LE:32:default:watcom"" ENDIAN=""little"" />
    <DATATYPES>");

        foreach (var typeXml in _dataTypes)
            sb.AppendLine(typeXml);

        sb.AppendLine(@"    </DATATYPES>
</PROGRAM>
");
        return sb.ToString();
    }

    public ProgramData Load()
    {
        var xml = Build();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return ProgramData.Load(stream);
    }
}