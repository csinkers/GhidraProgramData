namespace GhidraProgramData;

public interface IGhidraType
{
    TypeKey Key { get; }
    uint? FixedSize { get; }
    string? BuildPath(string accum, string relative);
    bool Unswizzle(TypeStore types); // Return true if any types were resolved
}