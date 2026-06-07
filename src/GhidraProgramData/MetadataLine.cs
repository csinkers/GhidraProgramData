using System.Globalization;

namespace GhidraProgramData;

public struct MetadataLine
{
    public const string Pattern = "//!M! ";
    public uint BaseOffset;
    public string Name;
    public uint[] LineAddresses;
    public uint StackOffset;
    public uint[] ExitPoints;

    public static bool TryParse(ref MetadataLine result, ReadOnlySpan<char> line)
    {
        // Format: !M! {baseOffset:hex};{name};{lineOffsets:base64};{stackOffset:hex};{exitPoints:base64}

        if (!line.StartsWith(Pattern))
            return false;

        if (line.Length < Pattern.Length + 8)
            return false;

        line = line[Pattern.Length..];

        int i = 0;
        foreach (Range part in line.Split(';'))
        {
            switch (i)
            {
                case 0: // baseOffset
                    if (!uint.TryParse(line[part], NumberStyles.HexNumber, null, out result.BaseOffset))
                        return false;
                    break;

                case 1: // name
                    result.Name = line[part].ToString();
                    break;

                case 2: // lineOffsets
                    int[] offsets = PackedBase64Offsets.Decode(line[part]);
                    result.LineAddresses = PackedBase64Offsets.ConvertToAbsolute(result.BaseOffset, offsets);
                    break;

                case 3: // stackOffset
                    if (!uint.TryParse(line[part], NumberStyles.HexNumber, null, out result.StackOffset))
                        return false;
                    break;

                case 4: // exitPoints
                    int[] exitOffsets = PackedBase64Offsets.Decode(line[part]);
                    result.ExitPoints = PackedBase64Offsets.ConvertToAbsolute(result.BaseOffset, exitOffsets);
                    break;
            }

            i++;

        }

        return true;
    }
}
