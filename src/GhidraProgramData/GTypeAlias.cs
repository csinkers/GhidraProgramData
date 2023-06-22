namespace GhidraProgramData;

public class GTypeAlias : IGhidraType
{
    public TypeKey Key { get; }
    public IGhidraType Type { get; private set; }
    public uint? FixedSize => Type.FixedSize;
    public string? BuildPath(string accum, string relative) => Type.BuildPath(accum, relative);
    public override string ToString() => $"{Key.Name} = {Type}";

    public GTypeAlias(TypeKey key, IGhidraType type)
    {
        Key = key;
        Type = type;
    }

    public bool Unswizzle(TypeStore types)
    {
        if (Type is not GDummy dummy)
            return false;

        Type = types[dummy.Key];
        return true;
    }
}