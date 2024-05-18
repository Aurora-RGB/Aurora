using System.Collections;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Common.Data;

public sealed class MemorySharedArray<T> : SignaledMemoryObject, IEnumerable<T> where T : struct
{
    private const bool CheckFileSize = false;
    public int Count { get; }

    private static readonly int ElementSize = Marshal.SizeOf(typeof(T));

    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly byte[] _readBuffer = new byte[ElementSize];
    private readonly byte[] _writeBuffer = new byte[ElementSize];

    private readonly GCHandle _writeHandle;
    private readonly GCHandle _readHandle;

    private readonly IntPtr _writePointer;
    private readonly IntPtr _readPointer;

    public MemorySharedArray(string fileName) : base(fileName)
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

        _writeHandle = GCHandle.Alloc(_writeBuffer, GCHandleType.Pinned);
        _writePointer = _writeHandle.AddrOfPinnedObject();
        
        _readHandle = GCHandle.Alloc(_readBuffer, GCHandleType.Pinned);
        _readPointer = _readHandle.AddrOfPinnedObject();
    }

    public MemorySharedArray(string fileName, int size) : base(fileName)
    {
        Count = size;

        // Calculate the total size for the MemoryMappedFile
        long totalSize = ElementOffset() + Count * ElementSize;

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

        _writeHandle = GCHandle.Alloc(_writeBuffer, GCHandleType.Pinned);
        _writePointer = _writeHandle.AddrOfPinnedObject();
        
        _readHandle = GCHandle.Alloc(_readBuffer, GCHandleType.Pinned);
        _readPointer = _readHandle.AddrOfPinnedObject();
        
        // Initialize and write data to the MemoryMappedFile
        var data = default(T);
        for (var i = 0; i < Count; i++)
        {
            // Calculate the offset for the current element
            var offset = ElementOffset() + i * ElementSize;

            // Write the data at the calculated offset
            WriteObject(offset, data);
            //_accessor.Write(offset, ref data);
        }

        _accessor = _mmf.CreateViewAccessor();
        
        SignalUpdated();
    }

    public T ReadElement(int index)
    {
        // Create a MemoryMappedViewAccessor to read data
        // Calculate the offset for the specified element
        long offset = ElementOffset() + index * ElementSize;

        if (!_accessor.CanWrite || Disposed)
        {
            return default;
        }
        
        // Read the data back
        _accessor.ReadArray(offset, _readBuffer, 0, _readBuffer.Length);

        // Marshal the byte array back to a struct
        return (T)Marshal.PtrToStructure(_readPointer, typeof(T));
    }

    public void WriteDictionary<E>(IReadOnlyDictionary<E, T> dictionary) where E : Enum
    {
        foreach (var pair in dictionary)
        {
            // Count + E
            var offset = Convert.ToInt64(pair.Key) * ElementSize;
            if (offset < 0)
            {
                continue;
            }

            // Write the data at the calculated offset
            var element = pair.Value;
            if (!_accessor.CanWrite || Disposed)
            {
                return;
            }
            _accessor.Write(ElementOffset() + offset, ref element);
        }

        SignalUpdated();
    }

    public void WriteCollection(IEnumerable<T> list)
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
            WriteObject(ElementOffset() + offset, e);
        }

        SignalUpdated();
    }

    private static int ElementOffset()
    {
        return sizeof(long) + sizeof(int);
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        base.Dispose(disposing);

        _mmf.Dispose();
        _accessor.Dispose();
        _writeHandle.Free();
        _readHandle.Free();
    }

    private void WriteObject(int offset, T element)
    {
        // Marshal the struct to a byte array
        Marshal.StructureToPtr(element, _writePointer, true);

        if (!_accessor.CanWrite || Disposed)
        {
            return;
        }
        _accessor.WriteArray(offset, _writeBuffer, 0, _writeBuffer.Length);
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