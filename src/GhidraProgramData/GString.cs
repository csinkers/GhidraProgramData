namespace GhidraProgramData;

public class GString : IGhidraType
{
    GString() { }
    public static readonly GString Instance = new();
    public TypeKey Key => new("/", "string");
    public uint? FixedSize => null;
    public string? BuildPath(string accum, string relative) => null;
    public bool Unswizzle(TypeStore types) { return false; }
    public override string ToString() => "string";
}