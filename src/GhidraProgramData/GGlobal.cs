namespace GhidraProgramData;

public class GGlobal : IGhidraType
{
    public GGlobal(uint address, uint size, IGhidraType type)
    {
        Address = address;
        Size = size;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public TypeKey Key { get; set; } = new("", "");
    public uint Address { get; }
    public uint Size { get; }
    public IGhidraType Type { get; private set; }
    public uint? FixedSize => Size;
    public override string ToString() => $"{Type} @ {Address:X} ({Size:X})";
    public string? BuildPath(string accum, string relative) => Type.BuildPath(accum, relative);

    public bool Unswizzle(TypeStore types)
    {
        if (Type is not GDummy dummy) 
            return false;

        Type = types.Get(dummy.Key);
        return true;
    }
}