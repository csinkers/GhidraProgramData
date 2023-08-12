namespace GhidraProgramData;

public readonly record struct TypeKey(string Namespace, string Name)
{
    public override string ToString() => $"{Namespace}/{Name}";

    public static TypeKey Parse(string s)
    {
        int index = s.LastIndexOf('/');
        return index == -1 
            ? new TypeKey("", s) 
            : new TypeKey(s[..index], s[(index + 1)..]);
    }
}