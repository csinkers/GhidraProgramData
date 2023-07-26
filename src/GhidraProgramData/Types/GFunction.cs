using System.Text.RegularExpressions;

namespace GhidraProgramData.Types;

public record GFunction(TypeKey Key, uint Address)
{
    static readonly Regex DefaultName = new(@"^(FUN|LAB)_[0-9a-f]{8}$", RegexOptions.Compiled);

    public int Index { get; set; }
    public bool IsNamed => !DefaultName.IsMatch(Key.Name);
    public List<(uint Start, uint End)> Regions { get; } = new();
    public HashSet<GFunction> Callees { get; } = new();
    public HashSet<GFunction> Callers { get; } = new();
    public bool IsIgnored { get; set; }

    IEnumerable<int> Indices(IEnumerable<GFunction> functions)
    {
        int lastIndex = Index;
        foreach (var c in functions.OrderBy(x => x.Index))
        {
            yield return c.Index - lastIndex;
            lastIndex = c.Index;
        }
    }

    public override string ToString()
    {
        var callees = string.Join(", ", Indices(Callees));
        var callers = string.Join(", ", Indices(Callers));
        return $"{Callees.Count:D3} {Callers.Count:D3} {Key.Name}: {callees} | {callers}";
    }
}