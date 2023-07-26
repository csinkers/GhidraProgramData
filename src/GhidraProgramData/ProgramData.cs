using GhidraProgramData.Types;

namespace GhidraProgramData;

public class ProgramData
{
    internal ProgramData(GNamespace rootNamespace, Symbol[] symbols, GFunction[] functions, Dictionary<TypeKey, GFunction> functionsByName, TypeStore types)
    {
        Root = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        Functions = functions ?? throw new ArgumentNullException(nameof(functions));
        FunctionsByName = functionsByName ?? throw new ArgumentNullException(nameof(functionsByName));
        Types = types ?? throw new ArgumentNullException(nameof(types));
    }

    public GNamespace Root { get; }
    public GFunction[] Functions { get; } // All functions ordered by address
    public Dictionary<TypeKey, GFunction> FunctionsByName { get; }
    public Symbol[] Symbols { get; } // All symbols ordered by address
    public TypeStore Types { get; }

    public static ProgramData Load(string xmlPath, string? asciiDisassemblyPath = null, Func<string, bool>? functionFilter = null)
    {
        using var xmlSr = new StreamReader(xmlPath);
        using var asciiSr = asciiDisassemblyPath == null ? null : new StreamReader(xmlPath);
        var loader = new ProgramDataLoader();
        return loader.Load(xmlSr, asciiSr, functionFilter);
    }

    public static ProgramData Load(Stream xmlStream, Stream? asciiDisassemblyStream = null, Func<string, bool>? functionFilter = null)
    {
        using var xmlSr = new StreamReader(xmlStream);
        using var asciiSr = asciiDisassemblyStream == null ? null : new StreamReader(asciiDisassemblyStream);
        var loader = new ProgramDataLoader();
        return loader.Load(xmlSr, asciiSr, functionFilter);
    }

    public Symbol? LookupSymbol(uint address)
    {
        if (address == 0 || Symbols.Length == 0)
            return null;

        var index = Util.FindNearest(Symbols, x => x.Address, address);
        return Symbols[index];
    }

    public GFunction? LookupFunction(uint address)
    {
        if (address == 0)
            return null;

        var index = Util.FindNearest(Functions, x => x.Address, address);
        return Functions[index];
    }
}