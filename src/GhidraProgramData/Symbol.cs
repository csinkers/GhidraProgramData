namespace GhidraProgramData;

public record Symbol(uint Address, string Namespace, string Name)
{
    public object? Context { get; internal set; }
    public TypeKey Key => new(Namespace, Name);
}