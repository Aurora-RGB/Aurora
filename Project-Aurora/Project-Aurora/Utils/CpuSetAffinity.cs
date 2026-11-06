using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AuroraRgb.Settings;

namespace AuroraRgb.Utils;

public sealed record CpuSet(uint Id, ushort Group, byte LogicalProcessor, byte Core,
    byte EfficiencyClass, bool Available)
{
    public string Key => $"{Group}:{LogicalProcessor}";
    public string Name => $"Group {Group}, CPU {LogicalProcessor} · Core {Core} · Efficiency class {EfficiencyClass}";
}

/// <summary>Process defaults cover existing and future threads without a per-thread polling loop.</summary>
public static partial class CpuSetAffinity
{
    public static IReadOnlyList<CpuSet> GetCpuSets()
    {
        const int insufficientBuffer = 122;
        var process = new IntPtr(-1); // GetCurrentProcess pseudo handle; never close it.
        if (!GetSystemCpuSetInformation(IntPtr.Zero, 0, out var length, process, 0) &&
            Marshal.GetLastPInvokeError() != insufficientBuffer)
            throw new Win32Exception(Marshal.GetLastPInvokeError());

        // Retry if processors are added between the size query and the data query.
        for (var attempt = 0; attempt < 3 && length > 0; attempt++)
        {
            var capacity = length;
            var buffer = Marshal.AllocHGlobal(checked((int)capacity));
            try
            {
                if (!GetSystemCpuSetInformation(buffer, capacity, out length, process, 0))
                {
                    var error = Marshal.GetLastPInvokeError();
                    if (error == insufficientBuffer)
                        continue;
                    throw new Win32Exception(error);
                }

                if (length > capacity)
                    throw new InvalidDataException("Invalid CPU topology size.");
                var sets = new List<CpuSet>();
                for (var offset = 0; offset < length;)
                {
                    if (length - offset < 8)
                        throw new InvalidDataException("Incomplete CPU topology header.");
                    var entry = IntPtr.Add(buffer, offset);
                    var size = Marshal.ReadInt32(entry);
                    if (size < 8 || size > length - offset)
                        throw new InvalidDataException("Invalid CPU topology entry size.");

                    // SYSTEM_CPU_SET_INFORMATION is variable-sized; skip unknown entry types.
                    if (Marshal.ReadInt32(entry, 4) == 0 && size >= 32)
                    {
                        var flags = Marshal.ReadByte(entry, 19);
                        sets.Add(new CpuSet(unchecked((uint)Marshal.ReadInt32(entry, 8)),
                            unchecked((ushort)Marshal.ReadInt16(entry, 12)),
                            Marshal.ReadByte(entry, 14), Marshal.ReadByte(entry, 15),
                            Marshal.ReadByte(entry, 18),
                            (flags & 2) == 0 || (flags & 4) != 0));
                    }
                    offset += size;
                }
                return sets.OrderBy(c => c.Group).ThenBy(c => c.LogicalProcessor).ToArray();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        if (length == 0)
            return [];
        throw new Win32Exception(insufficientBuffer);
    }

    public static CpuSet[] Select(IReadOnlyList<CpuSet> topology, CpuAffinityMode mode,
        IReadOnlyCollection<string>? manual)
    {
        var available = topology.Where(c => c.Available).ToArray();
        if (available.Length == 0 || mode == CpuAffinityMode.WindowsDefault)
            return [];

        // Higher EfficiencyClass means faster, less efficient hardware. Never infer E-cores
        // from CPU numbering, SMT siblings, parking, or undocumented SchedulingClass values.
        var lowest = available.Min(c => c.EfficiencyClass);
        var highest = available.Max(c => c.EfficiencyClass);
        return mode switch
        {
            CpuAffinityMode.Automatic or CpuAffinityMode.EfficiencyCores when lowest < highest =>
                available.Where(c => c.EfficiencyClass == lowest).ToArray(),
            CpuAffinityMode.PerformanceCores when lowest < highest =>
                available.Where(c => c.EfficiencyClass == highest).ToArray(),
            CpuAffinityMode.PhysicalCores => available.GroupBy(c => (c.Group, c.Core))
                .Select(g => g.OrderBy(c => c.LogicalProcessor).First()).ToArray(),
            CpuAffinityMode.Manual => available.Where(c => manual?.Contains(c.Key) == true).ToArray(),
            _ => [], // No distinct core classes, invalid mode, or stale manual selection: Windows manages placement.
        };
    }

    public static unsafe void Apply(IReadOnlyList<CpuSet> selected)
    {
        var ids = selected.Select(c => c.Id).ToArray();
        fixed (uint* pointer = ids)
        {
            if (!SetProcessDefaultCpuSets(new IntPtr(-1), pointer, (uint)ids.Length))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemCpuSetInformation(IntPtr information, uint bufferLength,
        out uint returnedLength, IntPtr process, uint flags);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool SetProcessDefaultCpuSets(IntPtr process, uint* cpuSetIds, uint count);
}
