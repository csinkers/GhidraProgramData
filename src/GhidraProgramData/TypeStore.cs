namespace GhidraProgramData;

public class TypeStore
{
    readonly Dictionary<TypeKey, IGhidraType> _types = new();
    public List<string> Errors { get; } = new();
    public List<TypeKey> AllKeys => _types.Keys.ToList();

    public void Add(TypeKey key, IGhidraType type)
    {
        _types[key] = type;
    }

    public void Add(IGhidraType type)
    {
        _types[type.Key] = type;
    }

    public IGhidraType this[TypeKey key] => Get(key);
    public IGhidraType Get(TypeKey key)
    {
        if (_types.TryGetValue(key, out var type))
            return type;

        if (key.Name == "char *")
            return new GPointer(Get(key with { Name = "string" }));

        if (key.Name.EndsWith('*'))
        {
            var result = new GPointer(Get(key with { Name = key.Name[..^1].Trim() }));
            Add(key, result);
            return result;
        }

        int index = key.Name.IndexOf('[');
        if (index != -1)
        {
            int index2 = key.Name.IndexOf(']');
            var subString = key.Name[(index + 1)..index2];
            var count = uint.Parse(subString);
            var result = new GArray(Get(key with { Name = key.Name[..index] + key.Name[(index2 + 1)..] }), count);
            Add(key, result);
            return result;
        }

        return new GDummy(key);
    }
}