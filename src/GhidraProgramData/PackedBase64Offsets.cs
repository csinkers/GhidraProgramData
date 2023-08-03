using System.Text;

namespace GhidraProgramData;

internal static class PackedBase64Offsets
{
    public static int[] ConvertToOffsets(uint baseAddress, uint[] values)
    {
        var results = new int[values.Length];
        var last = baseAddress;
        for (int i = 0; i < values.Length; i++)
        {
            results[i] = (int)(values[i] - last);
            last = values[i];
        }
        return results;
    }

    public static uint[] ConvertToAbsolute(uint baseAddress, int[] offsets)
    {
        if (offsets.Length == 0)
            return Array.Empty<uint>();

        var results = new uint[offsets.Length];
        results[0] = (uint)(offsets[0] + baseAddress);
        for (int i = 1; i < offsets.Length; i++)
            results[i] = (uint)(results[i - 1] + offsets[i]);

        return results;
    }

    public static string Encode(int[] offsets)
    {
        const string base64Digits = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        static int GetDigit(int n, int d)
        {
            // 6 bits per digit, 32-bit number = ceil(32/6) = 6 digits
            var shiftBy = d * 6;
            var mask = 0x3f << shiftBy;
            var bits = (uint)n & mask;
            return (int)(bits >> shiftBy);
        }

        var sb = new StringBuilder();

        var digits = new int[6];
        foreach (var offset in offsets)
        {
            var n = offset < 0 ? -offset : offset;

            if (n < 64)
            {
                sb.Append(base64Digits[n]);
            }
            else
            {
                sb.Append('[');
                bool writing = false;
                for (int i = 0; i < 6; i++)
                {
                    digits[i] = GetDigit(n, 5 - i);
                    if (digits[i] != 0 || writing)
                    {
                        writing = true;
                        sb.Append(base64Digits[digits[i]]);
                    }
                }
                sb.Append(']');
            }

            if (offset < 0)
                sb.Append('-');
        }
        return sb.ToString();
    }

    public static int[] Decode(string s)
    {
        static bool TryGetDigit(char c, out int n)
        {
            switch (c)
            {
                case >= 'A' and <= 'Z': n = c - 'A'; return true;
                case >= 'a' and <= 'z': n = c - 'a' + 26; return true;
                case >= '0' and <= '9': n = c - '0' + 52; return true;
                case '+': n = 62; return true;
                case '/': n = 63; return true;
                default: n = 0; return false;
            }
        }

        int cur = 0;
        bool multichar = false;
        var results = new List<int>();

        foreach (var c in s)
        {
            if (c == '-')
            {
                if (results.Count > 0)
                    results[^1] = -results[^1];
                continue;
            }

            if (c == '[')
            {
                multichar = true;
                cur = 0;
                continue;
            }

            if (multichar)
            {
                if (c == ']')
                {
                    results.Add(cur);
                    multichar = false;
                    continue;
                }

                if (!TryGetDigit(c, out var d))
                    continue;

                cur *= 64;
                cur += d;
            }
            else
            {
                if (!TryGetDigit(c, out var d))
                    continue;
                results.Add(d);
            }
        }

        return results.ToArray();
    }
}