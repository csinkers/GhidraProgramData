namespace GhidraProgramData;

public class GFuncParameter
{
    public uint Ordinal { get; }
    public string Name { get; }
    public IGhidraType Type { get; private set; }
    public uint Size { get; }

    public GFuncParameter(uint ordinal, string name, uint size, IGhidraType type)
    {
        Ordinal = ordinal;
        Name = name;
        Type = type;
        Size = size;
    }

    public bool Unswizzle(TypeStore types)
    {
        if (Type is not GDummy dummy) 
            return false;

        Type = types[dummy.Key];
        return true;
    }
}