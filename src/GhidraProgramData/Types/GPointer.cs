namespace GhidraProgramData.Types;

public class GPointer : IGhidraType
{
    public GPointer(IGhidraType type) => Type = type ?? throw new ArgumentNullException(nameof(type));
    public TypeKey Key => Type.Key with { Name = $"{Type.Key.Name} *" };
    public IGhidraType Type { get; private set; }
    public uint? FixedSize => Constants.PointerSize;

    public string? BuildPath(string accum, string relative)
    {
        accum += '*';
        return Type.BuildPath(accum, relative);
    }

    public override string ToString() => Key.Name;

    public bool Unswizzle(TypeStore types)
    {
        if (Type is not GDummy dummy)
            return false;

        Type = types[dummy.Key];
        return true;
    }
}