using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSBAurora
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new SBAuroraReactive.LEDManager();
            using (manager)
            {
                var devices = manager.EnumConnectedDevices();
                Console.WriteLine($"Found {devices.Length} connected SB devices:");
                for (int i=0; i<devices.Length; i++)
                {
                    SBAuroraReactive.EnumeratedDevice currDev = devices[i];
                    Console.WriteLine($"{i}: '{currDev.friendlyName}'({currDev.deviceId.vid:X4},{currDev.deviceId.pid}) with {currDev.totalNumLeds} LEDs");
                }

                if (devices.Length <= 0)
                    return;

                int theDevNum = -1;
                for (; ;)
                {
                    Console.WriteLine("Choose number of device to open:");
                    string chosenDevNumStr = Console.ReadLine();
                    if (!Int32.TryParse(chosenDevNumStr, out theDevNum) || theDevNum < 0 || theDevNum >= devices.Length)
                        Console.WriteLine("Invalid device number specified (" + chosenDevNumStr + ")!");
                    else
                        break;
                }

                SBAuroraReactive.EnumeratedDevice devInfo = devices[theDevNum];
                Console.WriteLine("Opening device idx {0}...", theDevNum);

                bool deviceOpened = false;
                const int numGroupColumns = 2;
                var groupArr = new uint[devInfo.totalNumLeds * numGroupColumns];
                for (uint i=0; i<devInfo.totalNumLeds; i++)
                {
                    groupArr[(i*numGroupColumns) + 0] = 1; //1 LED in group
                    groupArr[(i*numGroupColumns) + 1] = i; //TODO: make proper array using real indices
                }
                var patternArr = new SBAuroraReactive.LedPattern[devInfo.totalNumLeds];
                for (uint i=0; i<devInfo.totalNumLeds; i++)
                    patternArr[i] = SBAuroraReactive.LedPattern.Static;

                var colorArr = new SBAuroraReactive.LedColour[devInfo.totalNumLeds];
                for (uint i = 0; i < devInfo.totalNumLeds; i++)
                {
                    colorArr[i].a = 0;
                    colorArr[i].r = 0;
                    colorArr[i].g = 0;
                    colorArr[i].b = 0;
                }

                var ledSettings = new SBAuroraReactive.LedSettings();
                ledSettings.persistentInDevice = false;
                ledSettings.globalPatternMode = false;
                ledSettings.pattern = SBAuroraReactive.LedPattern.Static;
                ledSettings.payloadData = new SBAuroraReactive.LedPayloadData();
                try
                {
                    manager.OpenDevice(devInfo, false);
                    deviceOpened = true;
                    Console.WriteLine("Device opened.");

                    var timerSettings = new SBAuroraReactive.TimerSettings();
                    timerSettings.dueTimeMs = 0;
                    timerSettings.periodicTimeDesiredMs = 33; //30fps

                    SBAuroraReactive.LEDManager.PeriodicTimerDelegate timerCode = (uint timerPeriodMs, ulong currentTimeTickMs, ulong totalTimeElapsedMs) => 
                    {
                        uint currentLedToLight = (uint)((totalTimeElapsedMs / (ulong)(timerPeriodMs)) % (ulong)(devInfo.totalNumLeds));
                        for (uint i=0; i<devInfo.totalNumLeds; i++)
                        {
                            if (i == currentLedToLight)
                            {
                                colorArr[i].a = 0xFF;
                                colorArr[i].r = 0xFF;
                                colorArr[i].g = 0xFF;
                                colorArr[i].b = 0xFF;
                            }
                            else
                            {
                                colorArr[i].a = 0;
                                colorArr[i].r = 0;
                                colorArr[i].g = 0;
                                colorArr[i].b = 0;
                            }                            
                        }

                        ledSettings.payloadData = manager.LedPayloadInitialize(ledSettings.payloadData.Value, devInfo.totalNumLeds, 1, 1);
                        ledSettings.payloadData = manager.LedPayloadFillupAll(ledSettings.payloadData.Value, devInfo.totalNumLeds, patternArr, numGroupColumns, groupArr, 1, 1, colorArr);
                        manager.SetLedSettings(ledSettings);
                        ledSettings.payloadData = manager.LedPayloadCleanup(ledSettings.payloadData.Value, devInfo.totalNumLeds);
                        return 0;
                    };

                    timerSettings = manager.RegisterTimerCallback(timerCode, timerSettings);
                    Console.WriteLine("Timer running.");
                    System.Threading.Thread.Sleep(30000);
                    manager.UnregisterTimerCallback();
                    Console.WriteLine("Timer unregistered.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
                finally
                {
                    if (deviceOpened)
                    {
                        Console.WriteLine("Device closed.");
                        manager.CloseDevice();
                    }
                }
            }
        }
    }
}
