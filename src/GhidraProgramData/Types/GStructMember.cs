namespace GhidraProgramData.Types;

public class GStructMember
{
    List<IDirective>? _directives;
    public string Name { get; }
    public IGhidraType Type { get; private set; }
    public uint Offset { get; }
    public uint Size { get; }
    public string? Comment { get; }
    public IReadOnlyList<IDirective> Directives => _directives == null ? Array.Empty<IDirective>() : _directives;

    public GStructMember(string name, IGhidraType type, uint offset, uint size, string? comment)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Offset = offset;
        Size = size;
        Comment = comment;
    }

    public override string ToString() => $"{Offset:X}: {Type.Key.Name} {Name} ({Size:X}){(Comment == null ? "" : " // " + Comment)}";

    public void AddDirective(IDirective directive)
    {
        _directives ??= new List<IDirective>();
        _directives.Add(directive);
    }

    public void AddDirectives(IEnumerable<IDirective> directives)
    {
        foreach (var directive in directives)
            AddDirective(directive);
    }

    public bool Unswizzle(TypeStore types)
    {
        bool result = false;
        foreach (var directive in Directives)
            result |= directive.Unswizzle(types);

        if (Type is not GDummy dummy)
            return result;

        Type = types[dummy.Key];

        return true;
    }
}