namespace GhidraProgramData;

public class GStruct : IGhidraType
{
    public List<GStructMember> Members { get; }
    public TypeKey Key { get; }
    public uint Size { get; }
    public uint? FixedSize { get; }
    public string[] MemberNames { get; }

    public GStruct(TypeKey key, uint size, List<GStructMember> members)
    {
        Key = key;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        Size = size;

        FixedSize = 0; // In case the loop below results in a recursive call back to this type
        FixedSize = CalculateFixedSize(members);

        MemberNames = Members.Select(x => $"[{x.Name}]".Replace("%", "%%")).ToArray();
    }

    static uint? CalculateFixedSize(List<GStructMember> members)
    {
        uint sum = 0;
        foreach (var m in members)
        {
            if (m.Type.FixedSize == null)
                return null;
            sum += m.Type.FixedSize.Value;
        }
        return sum;
    }

    public string? BuildPath(string accum, string relative)
    {
        int dotIndex = relative.IndexOf('.');
        var part = dotIndex == -1 ? relative : relative[..dotIndex];
        var remainder = dotIndex == -1 ? "" : relative[(dotIndex + 1)..];

        for (int i = 0; i < Members.Count; i++)
        {
            var member = Members[i];
            if (member.Name == part)
            {
                accum += '/';
                accum += i.ToString();
                return remainder.Length == 0 ? accum : member.Type.BuildPath(accum, remainder);
            }
        }

        return null;
    }

    public override string ToString() => Key.Name; //$"struct {Key.Namespace}::{Key.Name} ({Size:X})";
    public bool Unswizzle(TypeStore types)
    {
        bool changed = false;
        foreach (var member in Members)
            changed |= member.Unswizzle(types);
        return changed;
    }
}