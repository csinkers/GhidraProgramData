using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace GhidraProgramData;

public class ProgramData
{
    readonly Symbol[] _symbols;
    public GFunction[] Functions { get; } // All functions ordered by address
    public Dictionary<TypeKey, GFunction> FunctionLookup { get; } = new();
    public TypeStore Types { get; } = new();
    public GNamespace Root { get; }

    public (uint address, string name, object? context) Lookup(uint address)
    {
        if (address == 0)
            return (0, "(null)", null);

        var index = Util.FindNearest(_symbols, x => x.Address, address);
        var symbol = _symbols[index];
        return (symbol.Address, symbol.Name, symbol.Context);
    }

    public static ProgramData Load(string xmlPath) => Load(xmlPath, _ => true);
    public static ProgramData Load(Stream xmlStream) => Load(xmlStream, _ => true);
    public static ProgramData Load(string xmlPath, Func<string, bool> functionFilter)
    {
        using var xmlStream = new StreamReader(xmlPath);
        return new(xmlStream, functionFilter);
    }

    public static ProgramData Load(Stream xmlStream, Func<string, bool> functionFilter)
    {
        using var sr = new StreamReader(xmlStream);
        return new(sr, functionFilter);
    }

    ProgramData(StreamReader xmlStream, Func<string, bool> functionFilter)
    {
        var doc = new XmlDocument();
        doc.Load(xmlStream);

        Types.Add(GString.Instance);
        foreach (var primitive in GPrimitive.PrimitiveTypes)
            Types.Add(primitive);

        IGhidraType BuildDummyType(TypeKey key) => Types.Get(new(key.Namespace.Trim(), key.Name.Trim()));

        foreach (XmlNode enumDef in doc.SelectNodes("/PROGRAM/DATATYPES/ENUM")!)
        {
            var ns = StrAttrib(enumDef, "NAMESPACE") ?? "";
            var name = StrAttrib(enumDef, "NAME") ?? "";
            var key = new TypeKey(ns, name);
            var size = UIntAttrib(enumDef, "SIZE");

            var elems = new Dictionary<uint, string>();
            foreach (var elem in enumDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "ENUM_ENTRY"))
                elems[UIntAttrib(elem, "VALUE")] = StrAttrib(elem, "NAME") ?? "";

            var ge = new GEnum(key, size, elems);
            Types.Add(ge);
        }

        foreach (XmlNode typeDef in doc.SelectNodes("/PROGRAM/DATATYPES/TYPE_DEF")!)
        {
            var ns = StrAttrib(typeDef, "NAMESPACE") ?? "";
            var name = StrAttrib(typeDef, "NAME") ?? "";
            var key = new TypeKey(ns, name);

            var dtns = StrAttrib(typeDef, "DATATYPE_NAMESPACE") ?? "";
            var type = StrAttrib(typeDef, "DATATYPE") ?? "";
            var alias = new GTypeAlias(key, BuildDummyType(new(dtns, type)));

            var existing = Types.Get(key);
            if (existing != null && existing is not GDummy)
            {
                if (existing is not GPrimitive)
                    throw new InvalidOperationException($"The type {ns}/{name} is already defined as {existing}");
            }
            else Types.Add(alias);
        }

        foreach (XmlNode funcDef in doc.SelectNodes("/PROGRAM/DATATYPES/FUNCTION_DEF")!)
        {
            var ns = StrAttrib(funcDef, "NAMESPACE") ?? "";
            var name = StrAttrib(funcDef, "NAME") ?? "";
            var key = new TypeKey(ns, name);

            IGhidraType returnType = GPrimitive.Void;
            var returnTypeNode = funcDef.SelectSingleNode("RETURN_TYPE");
            if (returnTypeNode != null)
            {
                var dtns = StrAttrib(returnTypeNode, "DATATYPE_NAMESPACE") ?? "";
                var type = StrAttrib(returnTypeNode, "DATATYPE") ?? "";
                returnType = BuildDummyType(new(dtns, type));
            }

            List<GFuncParameter> parameters = new();
            foreach (var parameter in funcDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "PARAMETER"))
            {
                var ordinal = UIntAttrib(parameter, "ORDINAL");
                var paramName = StrAttrib(parameter, "NAME") ?? "";
                var size = UIntAttrib(parameter, "SIZE");
                var dtns = StrAttrib(parameter, "DATATYPE_NAMESPACE") ?? "";
                var type = StrAttrib(parameter, "DATATYPE") ?? "";
                parameters.Add(new GFuncParameter(ordinal, paramName, size, BuildDummyType(new(dtns, type))));
            }

            var td = new GFuncPointer(key, returnType, parameters);
            Types.Add(td);
        }

        foreach (XmlNode structDef in doc.SelectNodes("/PROGRAM/DATATYPES/STRUCTURE")!)
        {
            var ns = StrAttrib(structDef, "NAMESPACE") ?? "";
            var name = StrAttrib(structDef, "NAME") ?? "";
            var key = new TypeKey(ns, name);
            var size = UIntAttrib(structDef, "SIZE");

            var members = new List<GStructMember>();
            foreach (var member in structDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "MEMBER"))
            {
                var dns = StrAttrib(member, "DATATYPE_NAMESPACE") ?? "";
                var dname = StrAttrib(member, "DATATYPE") ?? "";
                var type = BuildDummyType(new(dns, dname));
                var memberName = StrAttrib(member, "NAME");
                var offset = UIntAttrib(member, "OFFSET");

                if (string.IsNullOrEmpty(memberName))
                    memberName = $"unk{offset:X}";

                var commentNode = member.ChildNodes.OfType<XmlNode>().FirstOrDefault(x => x.Name == "REGULAR_CMT");
                members.Add(new GStructMember(memberName, type, offset, UIntAttrib(member, "SIZE"), commentNode?.InnerText));
            }

            Types.Add(new GStruct(key, size, members));
        }

        foreach (XmlNode unionDef in doc.SelectNodes("/PROGRAM/DATATYPES/UNION")!)
        {
            var ns = StrAttrib(unionDef, "NAMESPACE") ?? "";
            var name = StrAttrib(unionDef, "NAME") ?? "";
            var key = new TypeKey(ns, name);
            var size = UIntAttrib(unionDef, "SIZE");

            var members = new List<GStructMember>();
            foreach (var member in unionDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "MEMBER"))
            {
                var dns = StrAttrib(member, "DATATYPE_NAMESPACE") ?? "";
                var dname = StrAttrib(member, "DATATYPE") ?? "";

                var type = BuildDummyType(new(dns, dname));
                var memberName = StrAttrib(member, "NAME");
                var offset = UIntAttrib(member, "OFFSET");

                if (string.IsNullOrEmpty(memberName))
                    memberName = $"unk{offset:X}";

                members.Add(new GStructMember(memberName, type, offset, UIntAttrib(member, "SIZE"), StrAttrib(member, "COMMENT")));
            }


            Types.Add(new GUnion(key, size, members));
        }

        var symbols = new Dictionary<uint, Symbol>();

        Dictionary<TypeKey, GGlobal> globalVariables = new();
        foreach (XmlNode sym in doc.SelectNodes("/PROGRAM/SYMBOL_TABLE/SYMBOL")!)
        {
            var addr = HexAttrib(sym, "ADDRESS");
            if (addr == null)
                continue;

            var name = StrAttrib(sym, "NAME") ?? "";
            var ns = StrAttrib(sym, "NAMESPACE") ?? "";

            if (!name.StartsWith("case"))
                symbols[addr.Value] = new Symbol(addr.Value, ns, name);
        }

        foreach (XmlNode definedData in doc.SelectNodes("/PROGRAM/DATA/DEFINED_DATA")!)
        {
            var dns = StrAttrib(definedData, "DATATYPE_NAMESPACE") ?? "";
            var dname = StrAttrib(definedData, "DATATYPE") ?? "";
            uint? addr = HexAttrib(definedData, "ADDRESS");
            if (addr == null)
                continue;

            var size = UIntAttrib(definedData, "SIZE");
            var dataBlock = new GGlobal(addr.Value, size, BuildDummyType(new(dns, dname)));
            if (symbols.TryGetValue(addr.Value, out var symbol))
            {
                dataBlock.Key = symbol.Key;
                symbol.Context = dataBlock;
                globalVariables[dataBlock.Key] = dataBlock;
            }
        }

        var parser = new DirectiveParser(BuildDummyType);
        foreach (var key in Types.AllKeys)
        {
            var type = Types.Get(key);
            if (type is not GStruct structType) continue;
            foreach (var member in structType.Members)
            {
                if (member.Comment == null) continue;
                member.Directives = parser.TryParse(member.Comment, member.Name).ToList();
                if (member.Directives.Count == 0)
                    member.Directives = null;
            }
        }

        foreach (var key in Types.AllKeys)
        {
            var type = Types.Get(key);
            type.Unswizzle(Types);
        }

        foreach (var kvp in globalVariables)
            kvp.Value.Unswizzle(Types);

        var namespaces = new Dictionary<string, GNamespace>();
        Root = new GNamespace(Constants.RootNamespaceName);
        namespaces[""] = Root;

        GNamespace GetOrAddNamespace(string ns)
        {
            if (namespaces.TryGetValue(ns, out var result))
                return result;

            var parts = ns.Split('/');
            var cur = Root;
            foreach (var part in parts)
                cur = Root.GetOrAddNamespace(part);

            namespaces[ns] = cur;
            return cur;
        }

        foreach (var kvp in globalVariables)
        {
            var ns = GetOrAddNamespace(kvp.Key.Namespace);
            ns.Members.Add(kvp.Value);
        }

        Root.Sort();

        // Functions
        var regions = new List<(uint Start, uint End)>();

        foreach (XmlNode fn in doc.SelectNodes("/PROGRAM/FUNCTIONS/FUNCTION")!)
        {
            var addr = HexAttrib(fn, "ENTRY_POINT");
            if (addr == null)
                continue;

            var name = StrAttrib(fn, "NAME") ?? "";
            var ns = StrAttrib(fn, "NAMESPACE") ?? "";
            var key = new TypeKey(ns, name);

            regions.Clear();
            foreach (XmlNode range in fn.SelectNodes("ADDRESS_RANGE")!)
            {
                var begin = HexAttrib(range, "START");
                var end = HexAttrib(range, "END");

                if (begin == null || end == null)
                    continue;

                regions.Add((begin.Value, end.Value));
            }

            if (FunctionLookup.ContainsKey(key))
            {
                Console.WriteLine($"WARN: {key} already exists");
                continue;
            }

            var entry = new GFunction(key, addr.Value);
            foreach(var region in regions)
                entry.Regions.Add(region);

            FunctionLookup.Add(key, entry);
            if (symbols.TryGetValue(addr.Value, out var symbol))
                symbol.Context = entry;
        }

        Functions = FunctionLookup.Values.OrderBy(x => x.Address).ToArray();
        for (int i = 0; i < Functions.Length; i++)
        {
            var fn = Functions[i];
            fn.IsIgnored = !functionFilter(fn.Key.Name);
            fn.Index = i;
        }

        _symbols = symbols.Values.OrderBy(x => x.Address).ToArray();
    }

    static string? StrAttrib(XmlNode node, string name) => node.Attributes![name]?.Value;
    static uint UIntAttrib(XmlNode node, string name)
    {
        var value = node.Attributes![name]?.Value ?? throw new InvalidOperationException($"Attribute {name} was not found on node {node}!");
        return (value.StartsWith("0x"))
            ? uint.Parse(value[2..], NumberStyles.HexNumber)
            : uint.Parse(value);
    }

    static uint? HexAttrib(XmlNode node, string name)
    {
        var value = node.Attributes![name]?.Value ?? throw new InvalidOperationException($"Attribute {name} was not found on node {node}!");
        if (value.Contains(":")) // Skip entries like .image::OTHER:00000000 for headers in LE EXEs
            return null;

        return uint.Parse(value, NumberStyles.HexNumber);
    }

    static readonly Regex CallLineRegex = new(@"^([0-9a-f]{8})\s+[0-9a-f]+\s+CALL\s+([^ ]+)\s+$", RegexOptions.Compiled);
    readonly record struct CallInfo(uint Address, string Target);
    public void PopulateCalls(string path)
    {
        if (!File.Exists(path)) // Load ASCII dump as well if it exists to get call info
            return;

        var calls = new List<CallInfo>();

        using (var reader = new StreamReader(path))
        {
            while (reader.ReadLine() is { } line)
            {
                var m = CallLineRegex.Match(line);
                if (m.Success)
                {
                    var addr = uint.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                    calls.Add(new(addr, m.Groups[2].Value));
                }
            }
        }

        calls.Sort((x, y) => Comparer<uint>.Default.Compare(x.Address, y.Address));

        int index = 0;
        foreach (var fn in Functions)
        {
            foreach (var (start, end) in fn.Regions)
            {
                // If we've gone too far, rewind
                while (index >= calls.Count || index > 0 && calls[index].Address > start)
                    index--;

                // Skip any calls before the function
                for (; index < calls.Count; index++)
                {
                    var call = calls[index];

                    if (call.Address < start)
                        continue;

                    if (call.Address > end)
                        break;

                    if (call.Target.StartsWith("LAB_")) continue;
                    if (call.Target.Contains("=>")) continue;
                    switch (call.Target)
                    {
                        case "EAX":
                        case "EBX":
                        case "ECX":
                        case "EDX":
                        case "ESI":
                        case "EDI":
                            continue;
                    }

                    if (FunctionLookup.TryGetValue(new TypeKey("", call.Target), out var resolved))
                    {
                        if (!resolved.IsIgnored)
                        {
                            fn.Callees.Add(resolved);
                            resolved.Callers.Add(fn);
                        }
                    }
                    else
                        Console.WriteLine($"Could not resolve call target {call.Target}");
                }
            }
        }
    }
}