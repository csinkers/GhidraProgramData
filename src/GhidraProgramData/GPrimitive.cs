namespace GhidraProgramData;

public class GPrimitive : IGhidraType
{
    readonly uint _size;
    readonly string _name;

    public GPrimitive(string name, uint size)
    {
        _name = name;
        _size = size;
    }

    public TypeKey Key => new("/", _name);
    public uint? FixedSize => _size;
    public string? BuildPath(string accum, string relative) => null;
    public override string ToString() => _name;
    public bool Unswizzle(TypeStore types) { return false; }

    public static GPrimitive Bool { get; } = new("bool", 1);
    public static GPrimitive SByte { get; } = new("sbyte", 1);
    public static GPrimitive Word { get; } = new("word", 2);
    public static GPrimitive Short { get; } = new("short", 2);
    public static GPrimitive Int { get; } = new("int", 4);
    public static GPrimitive Long { get; } = new("long", 4);
    public static GPrimitive Dword { get; } = new("dword", 4);
    public static GPrimitive LongLong { get; } = new("longlong", 8);
    public static GPrimitive Qword { get; } = new("qword", 8);
    public static GPrimitive Byte { get; } = new("byte", 1);
    public static GPrimitive UChar { get; } = new("uchar", 1);
    public static GPrimitive UShort { get; } = new("ushort", 2);
    public static GPrimitive UInt { get; } = new("uint", 4);
    public static GPrimitive ULong { get; } = new("ulong", 4);
    public static GPrimitive ULongLong { get; } = new("ulonglong", 8);
    public static GPrimitive Undefined { get; } = new("undefined", 1);
    public static GPrimitive Undefined1 { get; } = new("undefined1", 1);
    public static GPrimitive Undefined2 { get; } = new("undefined2", 2);
    public static GPrimitive Undefined4 { get; } = new("undefined4", 4);
    public static GPrimitive Undefined6 { get; } = new("undefined6", 6);
    public static GPrimitive Undefined8 { get; } = new("undefined8", 8);
    public static GPrimitive Char { get; } = new("char", 1);
    public static GPrimitive Float { get; } = new("float", 4);
    public static GPrimitive Double { get; } = new("double", 8);
    public static GPrimitive Float10 { get; } = new("float10", 10);
    public static GPrimitive Void { get; } = new("void", 0);
    public static GPrimitive VaList { get; } = new("va_list", 0);
    public static GPrimitive ImageBaseOffset32 { get; } = new("ImageBaseOffset32", 4);
    public static GPrimitive Pointer32 { get; } = new("pointer32", 4);
    public static GPrimitive Pointer { get; } = new("pointer", Constants.PointerSize);
    public static GPrimitive SizeT { get; } = new("size_t", Constants.PointerSize);

    public static readonly GPrimitive[] PrimitiveTypes = {
        Bool,
        SByte, Word, Short, Int, Long, Dword, LongLong, Qword,
        Byte, UChar, UShort, UInt, ULong, ULongLong,
        Undefined, Undefined1, Undefined2, Undefined4, Undefined6, Undefined8,
        Char,
        Float, Double, Float10,
        Void,
        VaList,
        ImageBaseOffset32,
        Pointer32, Pointer,
        SizeT
    };
}