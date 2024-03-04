using System.Runtime.CompilerServices;

namespace NCollections.Generic;

public ref struct NativePinnableReference<TUnmanaged>
    where TUnmanaged : unmanaged
{
    private readonly unsafe TUnmanaged* _buffer;
    private readonly int _count;
    
    internal unsafe NativePinnableReference(void* buffer, int count)
    {
        if (count <= 0 || buffer == (void*)0)
        {
            this = default;

            return;
        }
        
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