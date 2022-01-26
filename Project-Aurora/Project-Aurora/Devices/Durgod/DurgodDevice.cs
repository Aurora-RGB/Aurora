using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using HidSharp;

namespace Aurora.Devices.Durgod
{
    class DurgodDevice : DefaultDevice
    {
        public override string DeviceName => "Durgod";

        HidDevice durgodKeyboard;
        HidStream packetStream;

        public byte[] colorCodeBytes = new byte[378];
        public byte[] prevColorCodeBytes = new byte[378];
        private int currentKeyOffset;


        public override bool Initialize()
        {

            foreach (int keyboardID in DurgodRGBMappings.KeyboardIDs)
            {
                durgodKeyboard = GetDurgodKeyboard(DurgodRGBMappings.DurgodID, keyboardID);
                if (durgodKeyboard != null)
                {
                    try
                    {
                        IsInitialized = durgodKeyboard.TryOpen(out packetStream);
                        packetStream.Write(DurgodRGBMappings.RgbOff);
                        for (int c = 0; c < colorCodeBytes.Length; c++) colorCodeBytes[c] = 0xff;
                    }
                    catch
                    {
                        IsInitialized = false;
                    }
                    break;
                }
                else
                {
                    IsInitialized = false;
                }
            }

            return IsInitialized;
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;
            try
            {
                packetStream.Write(DurgodRGBMappings.RgbOn);
            }
            catch { }
            
            packetStream?.Dispose();
            packetStream?.Close();
            IsInitialized = false;
        }


        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        { 

            int ROW_LENGTH = 42;
            int ROW_COUNT = 9;
            foreach (KeyValuePair<DeviceKeys, Color> kc in keyColors)
            {

                if (!DurgodRGBMappings.DurgodColourOffsetMap.TryGetValue(kc.Key, out currentKeyOffset))
                {
                    continue;
                }
                colorCodeBytes[currentKeyOffset * 3] = (byte)kc.Value.R;
                colorCodeBytes[currentKeyOffset * 3 + 1] = (byte)kc.Value.G;
                colorCodeBytes[currentKeyOffset * 3 + 2] = (byte)kc.Value.B;
            }

            if (!prevColorCodeBytes.SequenceEqual(colorCodeBytes))
            {
                //Firstly we must send message about starting color changing
                packetStream.Write(DurgodRGBMappings.RgbColormapStartMessage);
                //Durgod requires 9 separated message for each key block
                for (int row = 0; row < ROW_COUNT; row++)
                {
                    int start = row * ROW_LENGTH;
                    int end = (row + 1) * ROW_LENGTH;

                    byte[] segment = new byte[end - start];


                    byte[] colourHeader = { 0x00, 0x03, 0x18, 0x08, (byte)row };
                    byte[] header = new byte[47];

                    //Let's compose your message for keyboard
                    //Firstly we need magic bytes for color change
                    Array.Copy(colourHeader, 0, header, 0, colourHeader.Length);
                    //Secondly we need a slice which will appropriate to row
                    Array.Copy(colorCodeBytes, start, segment, 0, segment.Length);
                    //Finally we copy this slice to final message
                    Array.Copy(segment, 0, header, 5, segment.Length);
                    packetStream.Write(header);

                }

                colorCodeBytes.CopyTo(prevColorCodeBytes, 0);
                
                packetStream.Write(DurgodRGBMappings.RgbColormapEndMessage);
            }

            return true;
        }

        private HidDevice GetDurgodKeyboard(int VID, int PID) => DeviceList.Local.GetHidDevices(VID, PID).FirstOrDefault(HidDevice => HidDevice.GetMaxInputReportLength() == 65);
    }
}
