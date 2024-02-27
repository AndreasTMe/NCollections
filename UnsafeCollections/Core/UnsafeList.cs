using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnsafeCollections.Core;

[StructLayout(LayoutKind.Sequential)]
public struct UnsafeList : IEquatable<UnsafeList>, IDisposable
{
    private const int InitialCapacity = 4;

    private readonly RuntimeTypeHandle _typeHandle;

    private unsafe void* _buffer;
    private int _capacity;
    private int _count;

    public static UnsafeList Void { get; } = new();

    public readonly RuntimeTypeHandle TypeHandle => _typeHandle;

    public readonly int Capacity => _capacity;

    public readonly int Count => _count;

    public readonly bool IsEmpty => _count == 0;

    public readonly bool IsFull => _count == _capacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeList()
    {
        unsafe
        {
            _buffer = Unsafe.AsPointer(ref Unsafe.NullRef<byte>());
            _typeHandle = default;
            _capacity = _count = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeList(int capacity, Type type)
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
            _typeHandle = type.TypeHandle;
            _capacity = capacity;
            _count = 0;
        }
    }

    public static UnsafeList From<TUnmanaged>(in ReadOnlySpan<TUnmanaged> span)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if (span.Length <= 0)
            return Empty<TUnmanaged>();

        var list = new UnsafeList(span.Length, typeof(TUnmanaged));

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

    public static UnsafeList Empty<TUnmanaged>()
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged> =>
        new(0, typeof(TUnmanaged));

    public readonly bool TryGet<TUnmanaged>(int index, out TUnmanaged value, bool checkType = true)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if ((checkType && !typeof(TUnmanaged).TypeHandle.Equals(_typeHandle)) || (uint)index >= (uint)_count)
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

    public bool TryAdd<TUnmanaged>(in TUnmanaged item, bool checkType = true)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if (checkType && !typeof(TUnmanaged).TypeHandle.Equals(_typeHandle))
            return false;

        unsafe
        {
            if ((uint)_count >= (uint)_capacity)
                Expand(Unsafe.SizeOf<TUnmanaged>());

            var size = Unsafe.SizeOf<TUnmanaged>();

            Unsafe.CopyBlock(
                (byte*)_buffer + _count * size,
                Unsafe.AsPointer(ref Unsafe.AsRef(in item)),
                (uint)size);

            _count += 1;
        }

        return true;
    }

    public bool TryRemove<TUnmanaged>(in TUnmanaged item, bool checkType = true)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if (checkType && !typeof(TUnmanaged).TypeHandle.Equals(_typeHandle))
            return false;

        unsafe
        {
            for (var i = 0; i < _count; i++)
            {
                if (!Unsafe.Add(ref Unsafe.AsRef<TUnmanaged>(_buffer), i).Equals(item))
                    continue;

                return TryRemoveAt(i);
            }
        }

        return false;
    }

    public bool TryRemoveAt(int index)
    {
        if (_typeHandle.Equals(default) || (uint)index >= (uint)_count)
            return false;

        unsafe
        {
            if (index < _count - 1)
            {
                var size = Marshal.SizeOf(Type.GetTypeFromHandle(_typeHandle)!);

                Unsafe.CopyBlock(
                    (byte*)_buffer + index * size,
                    (byte*)_buffer + (index + 1) * size,
                    (uint)((_count - index - 1) * size));
            }
        }

        _count -= 1;

        return true;
    }

    public bool Contains<TUnmanaged>(in TUnmanaged item, bool checkType = true)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if (checkType && !typeof(TUnmanaged).TypeHandle.Equals(_typeHandle))
            return false;

        unsafe
        {
            for (var i = 0; i < _count; i++)
            {
                if (Unsafe.Add(ref Unsafe.AsRef<TUnmanaged>(_buffer), i).Equals(item))
                    return true;
            }
        }

        return false;
    }

    public bool TryGetIndexOf<TUnmanaged>(in TUnmanaged item, out int index, bool checkType = true)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if (checkType && !typeof(TUnmanaged).TypeHandle.Equals(_typeHandle))
        {
            index = -1;

            return false;
        }

        unsafe
        {
            for (var i = 0; i < _count; i++)
            {
                if (!Unsafe.Add(ref Unsafe.AsRef<TUnmanaged>(_buffer), i).Equals(item))
                    continue;

                index = i;

                return true;
            }
        }

        index = -1;

        return false;
    }

    public void Clear()
    {
        if (_count > 0)
            _count = 0;
    }

    public readonly Enumerator<TUnmanaged> AsEnumerator<TUnmanaged>(bool checkType = true)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if (checkType && !typeof(TUnmanaged).TypeHandle.Equals(_typeHandle))
            throw new InvalidOperationException("Enumerator data type mismatch.");

        unsafe
        {
            return new Enumerator<TUnmanaged>(_buffer, _count);
        }
    }

    public readonly PinnableReference<TUnmanaged> AsFixed<TUnmanaged>(bool checkType = true)
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        if (checkType && !typeof(TUnmanaged).TypeHandle.Equals(_typeHandle))
            throw new InvalidOperationException("Fixed pointer data type mismatch.");

        unsafe
        {
            return new PinnableReference<TUnmanaged>(_buffer, _count);
        }
    }

    private unsafe void Expand(int size)
    {
        _capacity = _capacity == 0 ? InitialCapacity : _capacity * 2;
        _buffer = NativeMemory.Realloc(_buffer, (nuint)(_capacity * size));
    }

    public void Dispose()
    {
        unsafe
        {
            if (Equals(Void))
                return;

            NativeMemory.Free(_buffer);
            this = Void;
        }
    }

    public readonly bool Equals(UnsafeList other)
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

    public override bool Equals(object? obj) => obj is UnsafeList other && Equals(other);

    public readonly override int GetHashCode()
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

    public readonly override string ToString() =>
        $"{nameof(UnsafeList)}[Count: {_count} | Capacity: {_capacity} | Type: {Type.GetTypeFromHandle(_typeHandle)?.Name ?? "Void"}]";

    public static bool operator ==(UnsafeList lhs, UnsafeList rhs) => lhs.Equals(rhs);

    public static bool operator !=(UnsafeList lhs, UnsafeList rhs) => !(lhs == rhs);

    public ref struct Enumerator<TUnmanaged>
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        private readonly unsafe TUnmanaged* _buffer;
        private readonly int _count;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator(void* buffer, int count)
        {
            _buffer = (TUnmanaged*)buffer;
            _count = count;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            var index = _index + 1;

            if ((uint)index >= (uint)_count)
            {
                return false;
            }

            _index = index;

            return true;
        }

        public readonly ref TUnmanaged Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    return ref _buffer[_index];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator<TUnmanaged> GetEnumerator() => this;
    }

    public ref struct PinnableReference<TUnmanaged>
        where TUnmanaged : unmanaged, IEquatable<TUnmanaged>
    {
        private readonly unsafe TUnmanaged* _buffer;
        private readonly int _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe PinnableReference(void* buffer, int count)
        {
            _buffer = (TUnmanaged*)buffer;
            _count = count;
        }

        public readonly ref TUnmanaged GetPinnableReference()
        {
            if (_count != 0)
            {
                unsafe
                {
                    return ref _buffer[0];
                }
            }

            return ref Unsafe.NullRef<TUnmanaged>();
        }
    }
}