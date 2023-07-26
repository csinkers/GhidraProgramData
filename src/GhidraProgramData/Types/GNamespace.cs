namespace GhidraProgramData.Types;

public class GNamespace : IGhidraType
{
    readonly string _name;
    public GNamespace(string name) => _name = name;
    public TypeKey Key => new(Constants.SpecialNamespace, _name);
    public uint? FixedSize => 0;
    public List<IGhidraType> Members { get; } = new();
    public bool Unswizzle(TypeStore types) => false;

    public string? BuildPath(string accum, string relative)
    {
        int slashIndex = relative.IndexOf('/');
        var part = slashIndex == -1 ? relative : relative[..slashIndex];
        var remainder = slashIndex == -1 ? "" : relative[(slashIndex + 1)..];

        for (int i = 0; i < Members.Count; i++)
        {
            var member = Members[i];
            if (member.Key.Name != part)
                continue;

            if (member is GNamespace ns)
            {
                accum += '/';
                accum += i.ToString();
                return remainder.Length == 0 ? accum : ns.BuildPath(accum, remainder);
            }

            if (member is GGlobal g)
            {
                accum += '/';
                accum += i.ToString();
                return remainder.Length == 0 ? accum : g.Type.BuildPath(accum, remainder);
            }
        }

        return null;
    }

    public bool Sort() // Return true if empty
    {
        Members.Sort((x, y) => Comparer<string>.Default.Compare(x.Key.Name, y.Key.Name));
        foreach (var ns in Members.OfType<GNamespace>())
            ns.Sort();

        return Members.Count == 0;
    }

    public GNamespace GetOrAddNamespace(string part)
    {
        var existing = Members.OfType<GNamespace>().FirstOrDefault(x => x.Key.Name == part);
        if (existing != null)
            return existing;

        var ns = new GNamespace(part);
        Members.Add(ns);
        return ns;
    }
}