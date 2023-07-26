using System.Text;

namespace GhidraProgramData.Types;

public class GArray : IGhidraType
{
    public uint? FixedSize => Type.FixedSize * Count;
    public IGhidraType Type { get; private set; }
    public uint Count { get; }
    public override string ToString() => Name;

    public GArray(IGhidraType type, uint count)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Count = count;
    }

    public bool Unswizzle(TypeStore types)
    {
        if (Type is not GDummy dummy)
            return false;

        Type = types[dummy.Key];
        return true;
    }

    string Name
    {
        get
        {
            IGhidraType type = this;
            StringBuilder sb = new();
            while (type is GArray array)
            {
                sb.Append('[');
                sb.Append(array.Count);
                sb.Append(']');
                type = array.Type;
            }

            return type.Key.Name + sb;
        }
    }

    public TypeKey Key => new(Type.Key.Namespace, Name);

    public string? BuildPath(string accum, string relative)
    {
        int dotIndex = relative.IndexOf('.');
        var part = dotIndex == -1 ? relative : relative[..dotIndex];
        var remainder = dotIndex == -1 ? "" : relative[(dotIndex + 1)..];

        if (!int.TryParse(part, out _))
            return null;

        accum += '/';
        accum += part;
        return remainder.Length == 0 ? accum : Type.BuildPath(accum, remainder);
    }
}