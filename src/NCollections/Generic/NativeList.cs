using NCollections.Internal.Extensions;
using NCollections.Internal.Helpers;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NCollections.Generic;

[StructLayout(LayoutKind.Sequential)]
public struct NativeList<TUnmanaged> : IEquatable<NativeList<TUnmanaged>>, IDisposable
    where TUnmanaged : unmanaged
{
    private const int InitialCapacity = 4;

    private unsafe TUnmanaged* _buffer;
    private int _capacity;
    private int _count;

    public static NativeList<TUnmanaged> Empty { get; } = new();

    public readonly int Capacity => _capacity;

    public readonly int Count => _count;

    public readonly bool IsEmpty => _count == 0;

    public readonly bool IsFull => _capacity > 0 && _capacity == _count;

    public NativeList()
    {
        unsafe
        {
            _buffer = (TUnmanaged*)Unsafe.AsPointer(ref Unsafe.NullRef<TUnmanaged>());
        }

        _capacity = _count = 0;
    }

    public NativeList(int capacity)
    {
        if (capacity <= 0)
        {
            this = Empty;

            return;
        }

        unsafe
        {
            _buffer = (TUnmanaged*)NativeMemory.AllocZeroed((nuint)capacity, (nuint)Unsafe.SizeOf<TUnmanaged>());
        }

        _capacity = capacity;
        _count = 0;
    }

    public NativeList(in ReadOnlySpan<TUnmanaged> span)
    {
        if (span.Length == 0)
        {
            this = Empty;

            return;
        }

        var length = span.Length;

        unsafe
        {
            _buffer = (TUnmanaged*)NativeMemory.AllocZeroed((nuint)length, (nuint)Unsafe.SizeOf<TUnmanaged>());

            fixed (TUnmanaged* pointer = span)
            {
                Unsafe.CopyBlock(_buffer, pointer, (uint)(length * Unsafe.SizeOf<TUnmanaged>()));
            }
        }

        _capacity = _count = length;
    }

    public readonly TUnmanaged this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_capacity)
                ThrowHelpers.IndexOutOfRangeException();

            unsafe
            {
                return _buffer[index];
            }
        }
        set
        {
            if ((uint)index >= (uint)_capacity)
                ThrowHelpers.IndexOutOfRangeException();

            unsafe
            {
                _buffer[index] = value;
            }
        }
    }

    public readonly bool TryGet(int index, out TUnmanaged item)
    {
        if ((uint)index >= (uint)_capacity)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in TUnmanaged item)
    {
        if ((uint)_count < (uint)_capacity)
        {
            unsafe
            {
                _buffer[_count] = item;
            }
        }
        else
        {
            ExpandAndAdd(item);
        }

        _count += 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private unsafe void ExpandAndAdd(in TUnmanaged item)
    {
        _capacity = _capacity == 0 ? InitialCapacity : _capacity * 2;
        _buffer = (TUnmanaged*)NativeMemory.Realloc(_buffer, (nuint)(_capacity * Unsafe.SizeOf<TUnmanaged>()));

        _buffer[_count] = item;
    }

    public bool TryRemove(in TUnmanaged item)
    {
        var index = IndexOf(item);

        if (index < 0)
            return false;

        TryRemoveAt(index);

        return true;
    }

    public bool TryRemoveAt(int index)
    {
        if ((uint)index >= (uint)_count)
            return false;

        unsafe
        {
            Unsafe.CopyBlock(
                _buffer + index,
                _buffer + index + 1,
                (uint)(_count - index - 1));
        }

        _count -= 1;

        return true;
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
    public void Clear()
    {
        if (_count > 0)
            _count = 0;
    }

    public NativeReadOnlyCollection<TUnmanaged> AsReadOnly()
    {
        NativeReadOnlyCollection<TUnmanaged> result;

        unsafe
        {
            result = new NativeReadOnlyCollection<TUnmanaged>(_buffer, _count);
        }

        Dispose();

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly NativeEnumerator<TUnmanaged> AsEnumerator()
    {
        unsafe
        {
            return new NativeEnumerator<TUnmanaged>(_buffer, _count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly NativePinnableReference<TUnmanaged> AsFixed()
    {
        unsafe
        {
            return new NativePinnableReference<TUnmanaged>(_buffer, _count);
        }
    }

    public void Dispose()
    {
        if (Equals(Empty))
            return;

        unsafe
        {
            NativeMemory.Free(_buffer);
            this = Empty;
        }
    }

    public bool Equals(NativeList<TUnmanaged> other)
    {
        unsafe
        {
            return _buffer == other._buffer
                && _capacity == other._capacity
                && _count == other._count
                && GetHashCode() == other.GetHashCode();
        }
    }

    public override bool Equals(object? obj) => obj is NativeList<TUnmanaged> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            const int hashingBase = (int)2166136261;
            const int hashingMultiplier = 16777619;

            var hash = hashingBase;

            hash = (hash * hashingMultiplier)
                ^ (!ReferenceEquals(null, typeof(NativeList<TUnmanaged>))
                    ? typeof(NativeList<TUnmanaged>).GetHashCode()
                    : 0);

            hash = (hash * hashingMultiplier)
                ^ (!ReferenceEquals(null, typeof(TUnmanaged)) ? typeof(TUnmanaged).GetHashCode() : 0);

            unsafe
            {
                hash = (hash * hashingMultiplier) ^ new IntPtr(_buffer).GetHashCode();
            }

            hash = (hash * hashingMultiplier) ^ _capacity.GetHashCode();
            hash = (hash * hashingMultiplier) ^ _count.GetHashCode();

            return hash;
        }
    }

    public readonly override string ToString() =>
        $"{nameof(NativeList<TUnmanaged>)}<{typeof(TUnmanaged).Name}>[Count: {_count} | Capacity: {_capacity}]";

    public static bool operator ==(NativeList<TUnmanaged> lhs, NativeList<TUnmanaged> rhs) => lhs.Equals(rhs);

    public static bool operator !=(NativeList<TUnmanaged> lhs, NativeList<TUnmanaged> rhs) => !(lhs == rhs);
}