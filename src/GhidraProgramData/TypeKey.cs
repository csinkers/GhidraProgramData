namespace GhidraProgramData;

public readonly record struct TypeKey
{
    public TypeKey(string ns, string name)
    {
        Namespace = ns is { Length: > 0 } && ns[0] == '/' ? ns[1..] : ns;
        Name = name;
    }

    public override string ToString() => $"{Namespace}/{Name}";

    public static TypeKey Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
            throw new FormatException("Cannot parse an empty type name");

        int index = s.LastIndexOf('/');
        if (s[0] == '/')
        {
            return (index == 0)
                ? new TypeKey("", s[1..])
                : new TypeKey(s[1..index], s[(index + 1)..]);
        }

        return index == -1
            ? new TypeKey("", s)
            : new TypeKey(s[..index], s[(index + 1)..]);
    }

    public string Namespace { get; init; }
    public string Name { get; init; }

    public void Deconstruct(out string ns, out string name)
    {
        ns = Namespace;
        name = Name;
    }
}