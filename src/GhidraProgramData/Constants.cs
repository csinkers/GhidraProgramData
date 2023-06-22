using System.Text;

namespace GhidraProgramData;

public static class Constants
{
    public const uint PointerSize = 4;
    public const string SpecialNamespace = "_";
    public const string RootNamespaceName = "";
    public static Encoding Encoding { get; }

    static Constants()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
        Encoding = Encoding.GetEncoding(850);
    }
}