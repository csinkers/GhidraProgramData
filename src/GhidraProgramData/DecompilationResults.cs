using System.Buffers.Text;
using System.Text;

namespace GhidraProgramData;

/// <summary>
/// Loads decompiled C-code exported by https://github.com/csinkers/GhidraLizardExport
/// </summary>
public class DecompilationResults : IDisposable
{
    readonly Dictionary<uint, long> _functionFileOffsets;
    readonly FileStream _stream;
    readonly StreamReader _streamReader;

    public static DecompilationResults Load(string path)
    {
        var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new DecompilationResults(stream);
    }

    DecompilationResults(FileStream stream)
    {
        // using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _streamReader = new StreamReader(stream, null, true, -1, true);
        _stream.Position = 0;
        _functionFileOffsets = IndexFunctions(stream);
    }

    public DecompiledFunction? TryGetFunction(uint address)
    {
        if (!_functionFileOffsets.TryGetValue(address, out var offset))
            return null;

        _stream.Position = offset;
        _streamReader.DiscardBufferedData();
        var metaLine = _streamReader.ReadLine();
        if (metaLine == null || !metaLine.StartsWith(Pattern))
            return null;

        if (metaLine.Length < Pattern.Length + 8)
            return null;

        string packedOffsets = metaLine[(Pattern.Length + 9)..]; // pattern + 8 chars for 32-bit hex number + 1 one more for a space, then the packed offsets start
        int[] offsets = PackedBase64Offsets.Decode(packedOffsets);
        uint[] lineAddresses = PackedBase64Offsets.ConvertToAbsolute(address, offsets);
        string[] lines = new string[lineAddresses.Length];

        for(int i = 0; i < lines.Length; i++)
            lines[i] = _streamReader.ReadLine()!;

        return new DecompiledFunction(address, lines, lineAddresses);
    }

    const string Pattern = "//!L! ";
    static Dictionary<uint, long> IndexFunctions(Stream stream)
    {
        byte[] pattern = Encoding.UTF8.GetBytes(Pattern);
        var lines = new Dictionary<uint, long>();

        Util.EnumerateLines(stream, pattern.Length + 8, (offset, line) =>
        {
            if (line.StartsWith(pattern) && Utf8Parser.TryParse(line[pattern.Length..], out uint funcAddress, out _, 'x'))
                lines[funcAddress] = offset;
        });

        return lines;
    }

    public void Dispose()
    {
        _streamReader.Dispose();
        _stream.Dispose();
    }
}