using System.Collections.Generic;

namespace NCollections.Extensions;

internal static class EqualityComparerExtensions
{
    public static unsafe int IndexOf<TUnmanaged>(
        this EqualityComparer<TUnmanaged> comparer,
        TUnmanaged* buffer,
        TUnmanaged value,
        int count)
        where TUnmanaged : unmanaged
    {
        for (var i = 0; i < count; i++)
            if (comparer.Equals(buffer[i], value))
                return i;

        return -1;
    }
}