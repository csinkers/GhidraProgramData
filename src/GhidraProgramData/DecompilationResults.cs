using System.Buffers.Text;
using System.Text;

namespace GhidraProgramData;

record struct FunctionOffset(long Offset, int LineNumber);

/// <summary>
/// Loads decompiled C-code exported by https://github.com/csinkers/GhidraLizardExport
/// </summary>
public sealed class DecompilationResults : IDisposable
{
    readonly Dictionary<uint, FunctionOffset> _functionFileOffsets;
    readonly Stream _stream;
    readonly StreamReader _streamReader;

    public static DecompilationResults Load(string path)
    {
        var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new DecompilationResults(stream);
    }

    public static DecompilationResults Load(Stream stream) => new(stream);

    DecompilationResults(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _streamReader = new StreamReader(stream, null, true, -1, true);
        _stream.Position = 0;
        _functionFileOffsets = IndexFunctions(stream);
    }

    public DecompiledFunction? TryGetFunction(uint address)
    {
        if (!_functionFileOffsets.TryGetValue(address, out var offset))
            return null;

        _stream.Position = offset.Offset;
        _streamReader.DiscardBufferedData();

        MetadataLine meta = default;
        var firstLine = _streamReader.ReadLine();
        if (firstLine == null || !MetadataLine.TryParse(ref meta, firstLine))
            return null;

        string[] lines = new string[meta.LineAddresses.Length];
        for(int i = 0; i < lines.Length; i++)
            lines[i] = _streamReader.ReadLine()!;

        return new DecompiledFunction(
            address,
            offset.LineNumber,
            meta.Name,
            lines,
            meta.LineAddresses,
            meta.StackOffset,
            meta.ExitPoints
        );
    }

    public IEnumerable<DecompiledFunction> EnumerateFunctions()
    {
        foreach (var address in _functionFileOffsets.Keys)
        {
            var func = TryGetFunction(address);
            if (func != null)
                yield return func;
        }
    }

    static Dictionary<uint, FunctionOffset> IndexFunctions(Stream stream)
    {
        byte[] pattern = Encoding.UTF8.GetBytes(MetadataLine.Pattern);
        var lines = new Dictionary<uint, FunctionOffset>();

        Util.EnumerateLines(stream, pattern.Length + 8, (offset, lineNumber, line) =>
        {
            if (line.StartsWith(pattern) && Utf8Parser.TryParse(line[pattern.Length..], out uint funcAddress, out _, 'x'))
                lines[funcAddress] = new FunctionOffset(offset, lineNumber);
        });

        return lines;
    }

    public void Dispose()
    {
        _streamReader.Dispose();
        _stream.Dispose();
    }
}