using System.Collections;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Common.Data;

public sealed class MemorySharedArrayRead<T> : SignaledMemoryObject, IEnumerable<T> where T : struct
{
    public int Count { get; }

    private static readonly int ElementSize = Marshal.SizeOf<T>();

    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly byte[] _readBuffer = new byte[ElementSize];

    private readonly GCHandle _readHandle;

    private readonly IntPtr _readPointer;

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

        //first long is byte length, second int is Count
        Count = _accessor.ReadInt32(sizeof(long));
        
        _readHandle = GCHandle.Alloc(_readBuffer, GCHandleType.Pinned);
        _readPointer = _readHandle.AddrOfPinnedObject();
    }

    public T ReadElement(int index)
    {
        // Create a MemoryMappedViewAccessor to read data
        // Calculate the offset for the specified element
        long offset = HeaderOffset() + index * ElementSize;

        if (!_accessor.CanWrite || Disposed)
        {
            return default;
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
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return ReadElement(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}