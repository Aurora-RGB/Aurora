using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Common.Data;

public sealed partial class MemorySharedArrayWrite<T> : SignaledMemoryObject where T : struct
{
    private const bool CheckFileSize = false;
    public int Count { get; }

    private static readonly int ElementSize = Marshal.SizeOf<T>();

    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly byte[] _elementWriteBuffer = new byte[ElementSize];

    private readonly GCHandle _elementWriteHandle;

    private readonly IntPtr _elementWritePointer;

    public MemorySharedArrayWrite(string fileName, int size) : base(fileName)
    {
        Count = size;

        // Calculate the total size for the MemoryMappedFile
        long totalSize = HeaderOffset() + Count * ElementSize;

        // Create a MemoryMappedFile
        _mmf = MemoryMappedFile.CreateOrOpen(fileName, totalSize);
        // Create a MemoryMappedViewAccessor to write data
        _accessor = _mmf.CreateViewAccessor();

        var setSize = _accessor.ReadInt32(0);
        if (CheckFileSize && setSize != totalSize && setSize != 0)
        {
            //TODO trigger an event to close all open files?
            _mmf.SafeMemoryMappedFileHandle.Close();
            _mmf.Dispose();
            _accessor.Dispose();
            _mmf = MemoryMappedFile.CreateNew(fileName, totalSize);
            _accessor = _mmf.CreateViewAccessor();
        }
        
        // Write array size
        _accessor.Write(0, totalSize);
        
        // Write array size
        _accessor.Write(sizeof(long), Count);

        _elementWriteHandle = GCHandle.Alloc(_elementWriteBuffer, GCHandleType.Pinned);
        _elementWritePointer = _elementWriteHandle.AddrOfPinnedObject();

        // Initialize and write data to the MemoryMappedFile
        var data = default(T);
        for (var i = 0; i < Count; i++)
        {
            // Calculate the offset for the current element
            var offset = HeaderOffset() + i * ElementSize;

            // Write the data at the calculated offset
            WriteObject(offset, data);
            //_accessor.Write(offset, ref data);
        }

        _accessor = _mmf.CreateViewAccessor();
        
        SignalUpdated();
    }

    public void WriteCollection(in IEnumerable<T> list)
    {
        var i = 0;
        foreach (var e in list)
        {
            var offset = i++ * ElementSize;
            if (offset < 0)
            {
                continue;
            }

            // Write the data at the calculated offset
            WriteObject(HeaderOffset() + offset, e);
        }

        SignalUpdated();
    }
    
    public void WriteArray(in T[] array)
    {
        if (!_accessor.CanWrite || Disposed) return;

        // Write the data at the calculated offset
        var offset = HeaderOffset();

        _accessor.WriteArray(offset, array, 0, array.Length);

        SignalUpdated();
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
        _elementWriteHandle.Free();
    }

    private void WriteObject(int offset, in T element)
    {
        if (!_accessor.CanWrite || Disposed)
        {
            return;
        }
        // Marshal the struct to a byte array
        Marshal.StructureToPtr(element, _elementWritePointer, true);

        if (!_accessor.CanWrite || Disposed)
        {
            return;
        }
        _accessor.WriteArray(offset, _elementWriteBuffer, 0, _elementWriteBuffer.Length);
    }
}