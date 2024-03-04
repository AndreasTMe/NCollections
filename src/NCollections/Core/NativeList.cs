using NCollections.Generic;
using NCollections.Internal.Extensions;
using NCollections.Internal.Helpers;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NCollections.Core;

[StructLayout(LayoutKind.Sequential)]
public struct NativeList : IEquatable<NativeList>, IDisposable
{
    private const int InitialCapacity = 4;

    private readonly RuntimeTypeHandle _typeHandle;

    private unsafe void* _buffer;
    private int _capacity;
    private int _count;

    public static NativeList Void { get; } = new();

    public readonly RuntimeTypeHandle TypeHandle => _typeHandle;

    public readonly int Capacity => _capacity;

    public readonly int Count => _count;

    public readonly bool IsEmpty => _count == 0;

    public readonly bool IsFull => _capacity > 0 && _count == _capacity;
    
    public NativeList()
    {
        unsafe
        {
            _buffer = Unsafe.AsPointer(ref Unsafe.NullRef<byte>());
        }

        _typeHandle = default;
        _capacity = _count = 0;
    }
    
    public NativeList(int capacity, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (capacity <= 0)
        {
            this = Void;
            _typeHandle = type.TypeHandle;

            return;
        }

        unsafe
        {
            _buffer = NativeMemory.AllocZeroed((nuint)capacity, (nuint)Marshal.SizeOf(type));
        }

        _typeHandle = type.TypeHandle;
        _capacity = capacity;
        _count = 0;
    }

    public static NativeList From<TUnmanaged>(in ReadOnlySpan<TUnmanaged> span)
        where TUnmanaged : unmanaged
    {
        var list = new NativeList(span.Length, typeof(TUnmanaged));

        if (span.Length <= 0)
            return list;

        unsafe
        {
            fixed (TUnmanaged* ptr = span)
            {
                Unsafe.CopyBlock(list._buffer, ptr, (uint)(span.Length * Unsafe.SizeOf<TUnmanaged>()));
            }
        }

        list._count = span.Length;

        return list;
    }

    public readonly bool TryGet<TUnmanaged>(int index, out TUnmanaged value, bool checkType = true)
        where TUnmanaged : unmanaged
    {
        if ((checkType && !IsOfType<TUnmanaged>()) || (uint)index >= (uint)_count)
        {
            value = default;

            return false;
        }

        unsafe
        {
            value = Unsafe.Add(ref Unsafe.AsRef<TUnmanaged>(_buffer), index);
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd<TUnmanaged>(in TUnmanaged item, bool checkType = true)
        where TUnmanaged : unmanaged
    {
        if (checkType && !IsOfType<TUnmanaged>())
            return false;

        if ((uint)_count < (uint)_capacity)
        {
            var size = Unsafe.SizeOf<TUnmanaged>();

            unsafe
            {
                Unsafe.CopyBlock(
                    (byte*)_buffer + _count * size,
                    Unsafe.AsPointer(ref Unsafe.AsRef(in item)),
                    (uint)size);
            }
        }
        else
        {
            ExpandAndAdd(item);
        }

        _count += 1;

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private unsafe void ExpandAndAdd<TUnmanaged>(in TUnmanaged item)
        where TUnmanaged : unmanaged
    {
        var size = Unsafe.SizeOf<TUnmanaged>();

        _capacity = _capacity == 0 ? InitialCapacity : _capacity * 2;
        _buffer = NativeMemory.Realloc(_buffer, (nuint)(_capacity * size));

        Unsafe.CopyBlock(
            (byte*)_buffer + _count * size,
            Unsafe.AsPointer(ref Unsafe.AsRef(in item)),
            (uint)size);
    }

    public bool TryRemove<TUnmanaged>(in TUnmanaged item, bool checkType = true)
        where TUnmanaged : unmanaged
    {
        if (checkType && !IsOfType<TUnmanaged>())
            return false;

        var index = IndexOf(item);

        if (index < 0)
            return false;

        TryRemoveAt<TUnmanaged>(index);

        return true;
    }

    public bool TryRemoveAt<TUnmanaged>(int index)
        where TUnmanaged : unmanaged
    {
        if (_typeHandle.Equals(default) || (uint)index >= (uint)_count)
            return false;

        var size = Unsafe.SizeOf<TUnmanaged>();

        unsafe
        {
            Unsafe.CopyBlock(
                (byte*)_buffer + index * size,
                (byte*)_buffer + (index + 1) * size,
                (uint)((_count - index - 1) * size));
        }

        _count -= 1;

        return true;
    }

    public bool Contains<TUnmanaged>(in TUnmanaged item, bool checkType = true)
        where TUnmanaged : unmanaged =>
        _count != 0 && IndexOf(item, checkType) != -1;

    public int IndexOf<TUnmanaged>(in TUnmanaged item, bool checkType = true)
        where TUnmanaged : unmanaged
    {
        if ((checkType && !IsOfType<TUnmanaged>()) || _count <= 0)
            return -1;

        // TODO: Check for bitwise equality

        unsafe
        {
            return EqualityComparer<TUnmanaged>.Default.IndexOf((TUnmanaged*)_buffer, item, _count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (_count > 0)
            _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly NativeEnumerator<TUnmanaged> AsEnumerator<TUnmanaged>(bool checkType = true)
        where TUnmanaged : unmanaged
    {
        if (checkType && !IsOfType<TUnmanaged>())
            ThrowHelpers.InvalidOperationException(ExceptionKey.EnumeratorTypeMismatch);

        unsafe
        {
            return new NativeEnumerator<TUnmanaged>(_buffer, _count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly NativePinnableReference<TUnmanaged> AsFixed<TUnmanaged>(bool checkType = true)
        where TUnmanaged : unmanaged
    {
        if (checkType && !IsOfType<TUnmanaged>())
            ThrowHelpers.InvalidOperationException(ExceptionKey.PinnableReferenceTypeMismatch);

        unsafe
        {
            return new NativePinnableReference<TUnmanaged>(_buffer, _count);
        }
    }

    private readonly bool IsOfType<TUnmanaged>()
        where TUnmanaged : unmanaged =>
        typeof(TUnmanaged).TypeHandle.Equals(_typeHandle);

    public void Dispose()
    {
        if (Equals(Void))
            return;

        unsafe
        {
            NativeMemory.Free(_buffer);
            this = Void;
        }
    }

    public readonly bool Equals(NativeList other)
    {
        unsafe
        {
            return _buffer == other._buffer
                && _typeHandle.Value == other._typeHandle.Value
                && _capacity == other._capacity
                && _count == other._count
                && GetHashCode() == other.GetHashCode();
        }
    }

    public override bool Equals(object? obj) => obj is NativeList other && Equals(other);

    public readonly override int GetHashCode()
    {
        unchecked
        {
            const int hashingBase = (int)2166136261;
            const int hashingMultiplier = 16777619;

            var hash = hashingBase;

            hash = (hash * hashingMultiplier)
                ^ (!ReferenceEquals(null, typeof(NativeList))
                    ? typeof(NativeList).GetHashCode()
                    : 0);

            unsafe
            {
                hash = (hash * hashingMultiplier) ^ new IntPtr(_buffer).GetHashCode();
            }

            hash = (hash * hashingMultiplier) ^ _typeHandle.GetHashCode();
            hash = (hash * hashingMultiplier) ^ _capacity.GetHashCode();
            hash = (hash * hashingMultiplier) ^ _count.GetHashCode();

            return hash;
        }
    }

    public readonly override string ToString() =>
        $"{nameof(NativeList)}[Count: {_count} | Capacity: {_capacity} | Type: {Type.GetTypeFromHandle(_typeHandle)?.Name ?? "Void"}]";

    public static bool operator ==(NativeList lhs, NativeList rhs) => lhs.Equals(rhs);

    public static bool operator !=(NativeList lhs, NativeList rhs) => !(lhs == rhs);
}