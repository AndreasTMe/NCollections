using System.Runtime.CompilerServices;

namespace NCollections.Generic;

public ref struct NativeEnumerator<TUnmanaged>
    where TUnmanaged : unmanaged
{
    private readonly unsafe TUnmanaged* _buffer;
    private readonly int _count;
    private int _index;

    internal unsafe NativeEnumerator(void* buffer, int count)
    {
        if (count <= 0 || buffer == (void*)0)
        {
            this = default;

            return;
        }

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
    public readonly NativeEnumerator<TUnmanaged> GetEnumerator() => this;
}