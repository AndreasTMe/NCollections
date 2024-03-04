using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NCollections.Helpers;

[ExcludeFromCodeCoverage]
internal static class ThrowHelpers
{
    [DoesNotReturn]
    public static void IndexOutOfRangeException() => throw new IndexOutOfRangeException();

    [DoesNotReturn]
    public static void InvalidOperationException(ExceptionKey key) =>
        throw new InvalidOperationException(GetExceptionMessage(key));

    internal static string? GetExceptionMessage(ExceptionKey key) =>
        key switch
        {
            ExceptionKey.None                          => null,
            ExceptionKey.EnumeratorTypeMismatch        => "Enumerator data type mismatch.",
            ExceptionKey.PinnableReferenceTypeMismatch => "Fixed pointer data type mismatch.",
            _                                          => throw new UnreachableException("This should never happen.")
        };
}