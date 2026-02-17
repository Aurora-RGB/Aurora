using System.Collections;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Common.Data;

public sealed class MemorySharedArrayRead<T> : SignaledMemoryObject, IEnumerable<T> where T : struct
{
    public int Count { get; }

    private static readonly int ElementSize = Marshal.SizeOf<T>();
    private static readonly T DefaultValue = default;

    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly byte[] _readBuffer = new byte[ElementSize];

    private readonly GCHandle _readHandle;

    private readonly IntPtr _readPointer;
    private readonly T[] _replicatedArray;
    private readonly GCHandle _replicatedArrayHandle;
    private readonly IntPtr _replicatedArrayPtr;
    private readonly int _replicatedArraySize;

    private readonly bool _typeBlittable;

    public MemorySharedArrayRead(string fileName) : base(fileName)
    {
        try
        {
            _mmf = MemoryMappedFile.OpenExisting(fileName);
            RequestUpdate();
        }
        catch (FileNotFoundException)
        {
            WaitForUpdate();
            _mmf = MemoryMappedFile.OpenExisting(fileName);
        }

        _accessor = _mmf.CreateViewAccessor();

        //the first long is byte length, the second int is Count
        Count = _accessor.ReadInt32(sizeof(long));

        _readHandle = GCHandle.Alloc(_readBuffer, GCHandleType.Pinned);
        _readPointer = _readHandle.AddrOfPinnedObject();

        _replicatedArray = new T[Count];
        _replicatedArraySize = Count * ElementSize;
        try
        {
            _replicatedArrayHandle = GCHandle.Alloc(_replicatedArray, GCHandleType.Pinned);
            _replicatedArrayPtr = _replicatedArrayHandle.AddrOfPinnedObject();
            _typeBlittable = true;
        }
        catch (ArgumentException)
        {
            // the type is not blittable
        }
    }

    public T ReadElement(int index)
    {
        // Create a MemoryMappedViewAccessor to read data
        // Calculate the offset for the specified element
        long offset = HeaderOffset() + index * ElementSize;

        if (!_accessor.CanWrite || Disposed)
        {
            return DefaultValue;
        }

        // Read the data back
        _accessor.ReadArray(offset, _readBuffer, 0, _readBuffer.Length);

        // Marshal the byte array back to a struct
        return Marshal.PtrToStructure<T>(_readPointer);
    }

    private static int HeaderOffset()
    {
        return sizeof(long) + sizeof(int);
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        base.Dispose(disposing);

        _mmf.Dispose();
        _accessor.Dispose();
        _readHandle.Free();
        _replicatedArrayHandle.Free();
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (!_typeBlittable)
        {
            // use the memory allocating method
            for (var i = 0; i < Count; i++)
            {
                yield return ReadElement(i);
            }
            yield break;
        }

        ReplicateShareArray();

        for (var i = 0; i < Count; i++)
        {
            yield return _replicatedArray[i];
        }
    }

    private unsafe void ReplicateShareArray()
    {
        // copy shared mem to _replicatedArray, without allocating new memory
        byte* srcPtr = null;

        _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref srcPtr);
        try
        {
            Buffer.MemoryCopy(
                srcPtr + HeaderOffset(),
                (void*)_replicatedArrayPtr,
                _replicatedArraySize,
                _replicatedArraySize
            );
        }
        finally
        {
            _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}