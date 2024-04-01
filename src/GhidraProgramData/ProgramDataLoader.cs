using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using GhidraProgramData.Directives;
using GhidraProgramData.Types;

namespace GhidraProgramData;

internal sealed class ProgramDataLoader
{
    static readonly Regex CallLineRegex = new(@"^([0-9a-f]{8})\s+[0-9a-f]+\s+CALL\s+([^ ]+)\s+$", RegexOptions.Compiled);
    readonly record struct CallInfo(uint Address, string Target);

    readonly Dictionary<TypeKey, GGlobal> _globalVariables = new();
    readonly TypeStore _types = new();

    IGhidraType BuildDummyType(TypeKey key) => _types.Get(new(key.Namespace.Trim(), key.Name.Trim()));

    public ProgramData Load(StreamReader xmlStream, StreamReader? asciiDisassemblyDumpReader, Func<string, bool>? functionFilter)
    {
        var doc = new XmlDocument();
        doc.Load(xmlStream);

        _types.Add(GString.Instance);
        foreach (var primitive in GPrimitive.PrimitiveTypes)
            _types.Add(primitive);

        Dictionary<uint, Symbol> symbols = new();
        LoadEnums(doc);
        LoadTypeDefs(doc);
        LoadFunctionPointerDefs(doc);
        LoadStructs(doc);
        LoadUnions(doc);
        LoadSymbols(doc, symbols);
        LoadGlobals(doc, symbols);

        // Load directives
        var parser = new DirectiveParser(BuildDummyType);
        foreach (var key in _types.AllKeys)
        {
            var type = _types.Get(key);
            if (type is not GStruct structType) continue;
            foreach (var member in structType.Members)
            {
                if (member.Comment == null) continue;
                member.AddDirectives(parser.TryParse(member.Comment, member.Name));
            }
        }

        // Resolve type references
        foreach (var key in _types.AllKeys)
        {
            var type = _types.Get(key);
            type.Unswizzle(_types);
        }

        foreach (var kvp in _globalVariables)
            kvp.Value.Unswizzle(_types);

        // Build namespace tree
        var namespaces = new Dictionary<string, GNamespace>();
        var rootNamespace = new GNamespace(Constants.RootNamespaceName);
        namespaces[""] = rootNamespace;

        GNamespace GetOrAddNamespace(string ns)
        {
            if (namespaces.TryGetValue(ns, out var result))
                return result;

            var parts = ns.Split('/');
            var cur = rootNamespace;
            foreach (var part in parts)
                cur = rootNamespace.GetOrAddNamespace(part);

            namespaces[ns] = cur;
            return cur;
        }

        foreach (var kvp in _globalVariables)
        {
            var ns = GetOrAddNamespace(kvp.Key.Namespace);
            ns.Members.Add(kvp.Value);
        }

        rootNamespace.Sort();

        Dictionary<TypeKey, GFunction> functions = new();
        LoadFunctions(doc, symbols, functions);

        var functionsArray = functions.Values.OrderBy(x => x.Address).ToArray();
        for (int i = 0; i < functionsArray.Length; i++)
        {
            var fn = functionsArray[i];
            if (functionFilter != null)
                fn.IsIgnored = !functionFilter(fn.Key.Name);
            fn.Index = i;
        }

        if (asciiDisassemblyDumpReader != null)
            PopulateCalls(asciiDisassemblyDumpReader, functions);

        return new ProgramData(
            rootNamespace,
            symbols.Values.OrderBy(x => x.Address).ToArray(),
            functionsArray,
            functions,
            _types);
    }

    void LoadEnums(XmlDocument doc)
    {
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
            _types.Add(ge);
        }
    }

    void LoadTypeDefs(XmlDocument doc)
    {
        foreach (XmlNode typeDef in doc.SelectNodes("/PROGRAM/DATATYPES/TYPE_DEF")!)
        {
            var ns = StrAttrib(typeDef, "NAMESPACE") ?? "";
            var name = StrAttrib(typeDef, "NAME") ?? "";
            var key = new TypeKey(ns, name);

            var dtns = StrAttrib(typeDef, "DATATYPE_NAMESPACE") ?? "";
            var type = StrAttrib(typeDef, "DATATYPE") ?? "";
            var alias = new GTypeAlias(key, BuildDummyType(new(dtns, type)));

            IGhidraType existing = _types.Get(key);
            if (existing is not GDummy)
            {
                if (existing is not GPrimitive)
                    throw new InvalidOperationException($"The type {ns}/{name} is already defined as {existing}");
            }
            else _types.Add(alias);
        }
    }

    void LoadFunctionPointerDefs(XmlDocument doc)
    {
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
            _types.Add(td);
        }
    }

    void LoadStructs(XmlDocument doc)
    {
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

            _types.Add(new GStruct(key, size, members));
        }
    }

    void LoadUnions(XmlDocument doc)
    {
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


            _types.Add(new GUnion(key, size, members));
        }
    }

    static void LoadSymbols(XmlDocument doc, Dictionary<uint, Symbol> symbols)
    {
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
    }

    void LoadGlobals(XmlDocument doc, Dictionary<uint, Symbol> symbols)
    {
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
                _globalVariables[dataBlock.Key] = dataBlock;
            }
        }
    }

    static void LoadFunctions(XmlDocument doc, Dictionary<uint, Symbol> symbols, Dictionary<TypeKey, GFunction> functionLookup)
    {
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

            if (functionLookup.ContainsKey(key))
            {
                Console.WriteLine($"WARN: {key} already exists");
                continue;
            }

            var entry = new GFunction(key, addr.Value);
            foreach (var region in regions)
                entry.Regions.Add(region);

            functionLookup.Add(key, entry);
            if (!symbols.TryGetValue(addr.Value, out var symbol))
            {
                symbol = new Symbol(addr.Value, key.Namespace, key.Name);
                symbols[addr.Value] = symbol;
            }

            symbol.Context = entry;
        }
    }

    static void PopulateCalls(StreamReader asciiDisassemblyDumpReader, Dictionary<TypeKey, GFunction> functions)
    {
        var calls = new List<CallInfo>();

        while (asciiDisassemblyDumpReader.ReadLine() is { } line)
        {
            var m = CallLineRegex.Match(line);
            if (m.Success)
            {
                var addr = uint.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                calls.Add(new(addr, m.Groups[2].Value));
            }
        }

        calls.Sort((x, y) => Comparer<uint>.Default.Compare(x.Address, y.Address));

        int index = 0;
        foreach (var kvp in functions)
        {
            var fn = kvp.Value;
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

                    if (functions.TryGetValue(new TypeKey("", call.Target), out var resolved))
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
        if (value.Contains(':')) // Skip entries like .image::OTHER:00000000 for headers in LE EXEs
            return null;

        return uint.Parse(value, NumberStyles.HexNumber);
    }
}