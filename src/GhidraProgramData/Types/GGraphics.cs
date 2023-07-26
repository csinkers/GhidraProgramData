namespace GhidraProgramData.Types;

public class GGraphics : IGhidraType
{
    public GGraphics(string width, string height, string stride, string palette)
    {
        Width = width;
        Height = height;
        Stride = stride;
        Palette = palette;
    }

    // Logical relative paths
    public string Width { get; }
    public string Height { get; }
    public string Stride { get; }
    public string Palette { get; }

    public TypeKey Key => new(Constants.SpecialNamespace, "gfx");
    public uint? FixedSize => null;
    public string? BuildPath(string accum, string relative) => null;
    public bool Unswizzle(TypeStore types) => false;
}

