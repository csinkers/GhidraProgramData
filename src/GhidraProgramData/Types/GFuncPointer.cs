namespace GhidraProgramData.Types;

public class GFuncPointer : IGhidraType
{
    public GFuncPointer(TypeKey key, IGhidraType returnType, List<GFuncParameter> parameters)
    {
        Key = key;
        ReturnType = returnType;
        Parameters = parameters;
    }

    public TypeKey Key { get; }
    public IGhidraType ReturnType { get; private set; }
    public List<GFuncParameter> Parameters { get; }
    public uint? FixedSize => Constants.PointerSize;
    public string? BuildPath(string accum, string relative) => null;

    public bool Unswizzle(TypeStore types)
    {
        bool changed = false;
        if (ReturnType is GDummy dummy)
        {
            ReturnType = types[dummy.Key];
            changed = true;
        }

        foreach (var p in Parameters)
            changed |= p.Unswizzle(types);
        return changed;
    }
}