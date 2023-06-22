namespace GhidraProgramData;

public class DTypeCast : IDirective
{
    public DTypeCast(IGhidraType type) { Type = type; }
    public IGhidraType Type { get; private set; }
    public bool Unswizzle(TypeStore types)
    {
        if (Type is not GDummy dummy)
            return false;
        
        Type = types[dummy.Key];
        return true;
    }
}