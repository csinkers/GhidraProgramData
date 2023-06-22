namespace GhidraProgramData;

public class GDummy : IGhidraType
{
    public TypeKey Key { get; }
    public GDummy(TypeKey key) => Key = key;
    public uint? FixedSize => 0;
    public string? BuildPath(string accum, string relative) => null;
    public override string ToString() => $"Dummy({Key.Namespace}, {Key.Name})";

    public bool Unswizzle(TypeStore types)
        => throw new InvalidOperationException(
            "Unswizzle should never be called on a dummy type - the calling type should recognise " +
            "it is a dummy in its own Unswizzle call and use the type dictionary to resolve it.");
}