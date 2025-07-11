﻿using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using AuroraRgb.Profiles.ETS2.GSI;

namespace AuroraRgb.Profiles.ETS2 {
    public class GameEvent_ETS2 : LightEvent {

        /// <summary>Name of the mapped field that the ETS2 Telemetry Server DLL outputs to.</summary>
        private const string memoryMappedFileName = "Local\\Ets2TelemetryServer";

        /// <summary>Target process name (so it can be changed for ATS).</summary>
        private string processName;

        private MemoryMappedFile _memFile;
        private MemoryMappedViewAccessor _memAccessor;

        public GameEvent_ETS2(string processName) : base() {
            this.processName = processName;
        }

        /// <summary>
        /// The MemoryMappedFile shared between this process and the ETS2 Telemetry Server.
        /// </summary>
        private MemoryMappedFile memFile {
            get {
                if (_memFile == null)
                    try { _memFile = MemoryMappedFile.OpenExisting(memoryMappedFileName, MemoryMappedFileRights.ReadWrite); }
                    catch { return null; }
                return _memFile;
            }
        }

        /// <summary>
        /// An Accessor for the shared MemoryMappedFile between this process and the ETS2 Telemetry Server.
        /// </summary>
        private MemoryMappedViewAccessor memAccessor {
            get {
                if (_memAccessor == null && memFile != null)
                    _memAccessor = memFile.CreateViewAccessor(0, Marshal.SizeOf(typeof(ETS2MemoryStruct)), MemoryMappedFileAccess.Read);
                return _memAccessor;
            }
        }

        public override void ResetGameState() {
            GameState = new GameState_ETS2(default(ETS2MemoryStruct));
        }

        public override void UpdateTick() {
            if (Process.GetProcessesByName(processName).Length > 0 && memAccessor != null) {
                // -- Below code adapted from the ETS2 Telemetry Server by Funbit (https://github.com/Funbit/ets2-telemetry-server) --
                IntPtr memPtr = IntPtr.Zero;

                try {
                    byte[] raw = new byte[Marshal.SizeOf(typeof(ETS2MemoryStruct))];
                    memAccessor.ReadArray(0, raw, 0, raw.Length);

                    memPtr = Marshal.AllocHGlobal(raw.Length);
                    Marshal.Copy(raw, 0, memPtr, raw.Length);
                    GameState = new GameState_ETS2((ETS2MemoryStruct)Marshal.PtrToStructure(memPtr, typeof(ETS2MemoryStruct)));
                }
                finally {
                    if (memPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(memPtr);
                }
                // -- End ETS2 Telemetry Server code --
            }
        }

        public override void SetGameState(IGameState new_game_state) { }
    }
}
