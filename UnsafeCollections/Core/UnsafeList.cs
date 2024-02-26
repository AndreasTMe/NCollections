using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnsafeCollections.Core;

[StructLayout(LayoutKind.Sequential)]
public struct UnsafeList : IEquatable<UnsafeList>, IDisposable
{
    private readonly IntPtr _typeHandle;

    private unsafe void* _buffer;
    private int _capacity;
    private int _count;

    public static UnsafeList Empty { get; } = new();

    public readonly IntPtr TypeHandle => _typeHandle;

    public readonly int Capacity => _capacity;

    public readonly int Count => _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeList()
    {
        unsafe
        {
            _buffer = Unsafe.AsPointer(ref Unsafe.NullRef<byte>());
            _typeHandle = IntPtr.Zero;
            _capacity = _count = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeList(int capacity, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (capacity <= 0)
        {
            this = Empty;

            return;
        }

        unsafe
        {
            _buffer = NativeMemory.AllocZeroed((nuint)capacity, (nuint)Marshal.SizeOf(type));
            _typeHandle = type.TypeHandle.Value;
            _capacity = capacity;
            _count = 0;
        }
    }

    public readonly bool TryGet<TUnmanaged>(int index, out TUnmanaged value)
        where TUnmanaged : unmanaged
    {
        if (typeof(TUnmanaged).TypeHandle.Value != _typeHandle || (uint)index >= (uint)_count)
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

    public bool TryAdd<TUnmanaged>(in TUnmanaged item)
        where TUnmanaged : unmanaged
    {
        if (typeof(TUnmanaged).TypeHandle.Value != _typeHandle)
            return false;

        unsafe
        {
            if ((uint)_count >= (uint)_capacity)
                Expand(Unsafe.SizeOf<TUnmanaged>());

            Unsafe.Add(ref Unsafe.AsRef<TUnmanaged>(_buffer), _count) = item;
            _count += 1;
        }

        return true;
    }

    // TODO: TryRemove
    // TODO: TryRemoveAt
    // TODO: Contains
    // TODO: IndexOf
    // TODO: Clear
    // TODO: GetEnumerator
    // TODO: GetPinnableReference

    private unsafe void Expand(int size)
    {
        _capacity = _capacity == 0 ? 4 : _capacity * 2;
        _buffer = NativeMemory.Realloc(_buffer, (nuint)(_capacity * size));
    }

    public void Dispose()
    {
        unsafe
        {
            if (Equals(Empty))
                return;

            NativeMemory.Free(_buffer);
            this = Empty;
        }
    }

    public bool Equals(UnsafeList other)
    {
        unsafe
        {
            return _buffer == other._buffer
                && _typeHandle == other._typeHandle
                && _capacity == other._capacity
                && _count == other._count
                && GetHashCode() == other.GetHashCode();
        }
    }

    public override bool Equals(object? obj) => obj is UnsafeList other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            const int hashingBase = (int)2166136261;
            const int hashingMultiplier = 16777619;

            var hash = hashingBase;

            hash = (hash * hashingMultiplier)
                ^ (!ReferenceEquals(null, typeof(UnsafeList))
                    ? typeof(UnsafeList).GetHashCode()
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

    public readonly override string ToString() => $"{nameof(UnsafeList)}[Count: {_count} | Capacity: {_capacity}]";

    public static bool operator ==(UnsafeList lhs, UnsafeList rhs) => lhs.Equals(rhs);

    public static bool operator !=(UnsafeList lhs, UnsafeList rhs) => !(lhs == rhs);
}