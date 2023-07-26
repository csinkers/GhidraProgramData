namespace GhidraProgramData.Types;

public class GEnum : IGhidraType
{
    public TypeKey Key { get; }
    public uint Size { get; }
    public uint? FixedSize => Size;
    public Dictionary<uint, string> Elements { get; }
    public override string ToString() => Key.Name;

    public GEnum(TypeKey key, uint size, Dictionary<uint, string> elements)
    {
        Key = key;
        Elements = elements;
        Size = size;
    }

    public string? BuildPath(string accum, string relative) => null;
    public bool Unswizzle(TypeStore types) { return false; }
}