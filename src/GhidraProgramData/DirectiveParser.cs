using System.Text.RegularExpressions;

namespace GhidraProgramData;

public class DirectiveParser
{
    readonly Func<TypeKey, IGhidraType> _buildDummyType;

    /* Syntax:

    #cast\(([^,]+), (.+)\) - perform typecast on member
    #gfx(width, height, stride, palette) - display as 8bpp graphics

    */
    static readonly Regex CastRegex = new(@"cast\(([^,]+),(.+)\)");
    static readonly Regex GfxRegex = new(@"gfx8\(([^,]+),([^,]+),([^,]+),([^,]+)\)");
    public DirectiveParser(Func<TypeKey, IGhidraType> buildDummyType) 
        => _buildDummyType = buildDummyType ?? throw new ArgumentNullException(nameof(buildDummyType));

    public IEnumerable<IDirective> TryParse(string comment, string memberName)
    {
        var parts = comment.Split('#');
        foreach (var part in parts.Skip(1))
        {
            var result =
                TryParseCast(part) ??
                TryParseGfx(part, memberName);

            if (result != null)
                yield return result;
        }
    }

    IDirective? TryParseCast(string comment)
    {
        var castMatch = CastRegex.Match(comment);
        if (!castMatch.Success) 
            return null;

        var path = castMatch.Groups[1].Value;
        var typeName = castMatch.Groups[2].Value;
        var parts = path.Split('.');
        var typeKey = SplitType(typeName);

        var dummyType = _buildDummyType(typeKey);
        IDirective directive = new DTypeCast(dummyType);

        for (int i = parts.Length - 1; i >= 0; i--)
            directive = new DTargetChild(parts[i].Trim(), directive);

        return directive;
    }

    static IDirective? TryParseGfx(string comment, string memberName)
    {
        var castMatch = GfxRegex.Match(comment);
        if (!castMatch.Success) 
            return null;

        var width = castMatch.Groups[1].Value.Trim();
        var height = castMatch.Groups[2].Value.Trim();
        var stride = castMatch.Groups[3].Value.Trim();
        var palette = castMatch.Groups[4].Value.Trim();

        var type = new GGraphics(width, height, stride, palette);
        var cast = new DTypeCast(type);
        return new DTargetChild(memberName, cast);
    }

    static TypeKey SplitType(string typeName)
    {
        var index = typeName.LastIndexOf('/');
        if (index == -1)
            return new("/", typeName);

        var ns = typeName[..index];
        var type = typeName[(index + 1)..];
        return new(ns, type);
    }
}