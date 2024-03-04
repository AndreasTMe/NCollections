using NCollections.Internal.Extensions;
using NCollections.Internal.Helpers;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NCollections.Generic;

public readonly struct NativeReadOnlyCollection<TUnmanaged>
    : IEquatable<NativeReadOnlyCollection<TUnmanaged>>, IDisposable
    where TUnmanaged : unmanaged
{
    private readonly unsafe TUnmanaged* _buffer;
    private readonly int _count;

    public int Count => _count;

    public bool IsEmpty => _count == 0;

    public static NativeReadOnlyCollection<TUnmanaged> Empty { get; } = new();

    public NativeReadOnlyCollection()
    {
        unsafe
        {
            _buffer = (TUnmanaged*)Unsafe.AsPointer(ref Unsafe.NullRef<TUnmanaged>());
        }

        _count = 0;
    }

    internal unsafe NativeReadOnlyCollection(TUnmanaged* buffer, int count)
    {
        if (count <= 0)
        {
            this = Empty;

            return;
        }

        _buffer = (TUnmanaged*)NativeMemory.AllocZeroed((nuint)count, (nuint)Unsafe.SizeOf<TUnmanaged>());
        Unsafe.CopyBlock(_buffer, buffer, (uint)(count * Unsafe.SizeOf<TUnmanaged>()));
        
        _count = count;
    }

    public ref TUnmanaged this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_count)
                ThrowHelpers.IndexOutOfRangeException();

            unsafe
            {
                return ref _buffer[index];
            }
        }
    }

    public bool TryGet(int index, out TUnmanaged item)
    {
        if ((uint)index >= (uint)_count)
        {
            item = default;

            return false;
        }

        unsafe
        {
            item = _buffer[index];

            return true;
        }
    }

    public bool Contains(in TUnmanaged item) => _count != 0 && IndexOf(item) != -1;

    public int IndexOf(in TUnmanaged item)
    {
        if (_count <= 0)
            return -1;

        // TODO: Check for bitwise equality

        unsafe
        {
            return EqualityComparer<TUnmanaged>.Default.IndexOf(_buffer, item, _count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeEnumerator<TUnmanaged> AsEnumerator()
    {
        unsafe
        {
            return new NativeEnumerator<TUnmanaged>(_buffer, _count);
        }
    }

    public void Dispose()
    {
        unsafe
        {
            if (Equals(Empty))
            {
                return;
            }

            NativeMemory.Free(_buffer);
        }
    }

    public bool Equals(NativeReadOnlyCollection<TUnmanaged> other)
    {
        unsafe
        {
            return _buffer == other._buffer && _count == other._count && GetHashCode() == other.GetHashCode();
        }
    }

    public override bool Equals(object? obj) => obj is NativeReadOnlyCollection<TUnmanaged> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            const int hashingBase = (int)2166136261;
            const int hashingMultiplier = 16777619;

            var hash = hashingBase;

            hash = (hash * hashingMultiplier)
                ^ (!ReferenceEquals(null, typeof(NativeReadOnlyCollection<TUnmanaged>))
                    ? typeof(NativeReadOnlyCollection<TUnmanaged>).GetHashCode()
                    : 0);

            hash = (hash * hashingMultiplier)
                ^ (!ReferenceEquals(null, typeof(TUnmanaged)) ? typeof(TUnmanaged).GetHashCode() : 0);

            unsafe
            {
                hash = (hash * hashingMultiplier) ^ new IntPtr(_buffer).GetHashCode();
            }

            hash = (hash * hashingMultiplier) ^ _count.GetHashCode();

            return hash;
        }
    }

    public override string ToString() =>
        $"{nameof(NativeReadOnlyCollection<TUnmanaged>)}<{typeof(TUnmanaged).Name}>[Count: {_count}]";

    public static bool operator ==(
        NativeReadOnlyCollection<TUnmanaged> lhs,
        NativeReadOnlyCollection<TUnmanaged> rhs) =>
        lhs.Equals(rhs);

    public static bool operator !=(
        NativeReadOnlyCollection<TUnmanaged> lhs,
        NativeReadOnlyCollection<TUnmanaged> rhs) =>
        !(lhs == rhs);
}