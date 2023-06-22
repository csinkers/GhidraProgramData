using System.Buffers;
using System.Numerics;

namespace GhidraProgramData;

public static class Util
{
    public static readonly long StartTimeTicks = DateTime.UtcNow.Ticks;

    public static float Timestamp(long ticks)
    {
        var dt = new DateTime(ticks);
        var startTime = new DateTime(StartTimeTicks);
        return (float)(dt - startTime).TotalSeconds;
    }

    public static ReadOnlySpan<T> SafeSlice<T>(ReadOnlySpan<T> span, uint from, uint size) => SafeSlice(span, (int)from, (int)size);
    public static ReadOnlySpan<T> SafeSlice<T>(ReadOnlySpan<T> span, int from, int size)
    {
        from = Math.Min(span.Length, from);
        size = Math.Min(span.Length - from, size);
        return span.Slice(from, size);
    }

    static readonly long MaxAgeTicks = TimeSpan.FromSeconds(3).Ticks;
    public static Vector4 ColorForAge(long ageInTicks)
    {
        if (ageInTicks >= MaxAgeTicks)
            return Vector4.One;

        var t = (float)ageInTicks / MaxAgeTicks;
        return new Vector4(1.0f, t, t, 1.0f);
    }

    public static int FindNearest<T>(IList<T> collection, Func<T, uint> addressAccessor, uint address) // Binary search
    {
        int first = 0;
        int last = collection.Count - 1;
        int mid;

        do
        {
            mid = first + (last - first) / 2;
            if (address > addressAccessor(collection[mid]))
                first = mid + 1;
            else
                last = mid - 1;

            if (addressAccessor(collection[mid]) == address)
                return mid;
        } while (first <= last);

        if (addressAccessor(collection[mid]) > address && mid != 0)
            mid--;

        return mid;
    }

    /// <summary>
    /// Calculate the difference between 2 strings using the Levenshtein distance algorithm
    /// Adapted from https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
    /// </summary>
    /// <param name="source1">First string</param>
    /// <param name="source2">Second string</param>
    /// <returns></returns>
    public static int LevenshteinDistance(string source1, string source2) //O(n*m)
    {
        var source1Length = source1.Length;
        var source2Length = source2.Length;
        var rows = source1Length + 1;
        var columns = source2Length + 1;
        var matrix = ArrayPool<int>.Shared.Rent(rows * columns);

        // First calculation, if one entry is empty return full length
        if (source1Length == 0)
            return source2Length;

        if (source2Length == 0)
            return source1Length;

        // Initialization of matrix with row size source1Length and columns size source2Length
        for (var i = 0; i <= source1Length; matrix[i] = i++) { }
        for (var j = 0; j <= source2Length; matrix[j * rows] = j++) { }

        // Calculate row and column distances
        for (var i = 1; i <= source1Length; i++)
        {
            for (var j = 1; j <= source2Length; j++)
            {
                var cost = source2[j - 1] == source1[i - 1] ? 0 : 1;

                matrix[j * rows + i] = Math.Min(
                    Math.Min(
                        matrix[j * rows + i - 1] + 1,
                        matrix[(j - 1) * rows + i] + 1),
                    matrix[(j - 1) * rows + (i - 1)] + cost);
            }
        }

        var result = matrix[source2Length * rows + source1Length];
        ArrayPool<int>.Shared.Return(matrix);
        return result;
    }
}