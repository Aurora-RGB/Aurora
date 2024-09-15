using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace AuroraRgb.Modules.HardwareMonitor;

public partial class HardwareMonitor
{
    public sealed class NetUpdater : HardwareUpdater
    {
        #region Sensors
        private ISensor? _bandwidthUsed;
        public float BandwidthUsed => GetValue(_bandwidthUsed);

        private ISensor? _uploadSpeed;
        public float UploadSpeedBytes => GetValue(_uploadSpeed);

        private ISensor? _downloadSpeed;
        public float DownloadSpeedBytes => GetValue(_downloadSpeed);

        private readonly List<IHardware> _networkDevices;
        #endregion

        public NetUpdater(IEnumerable<IHardware> hardware)
        {
            _networkDevices = hardware.Where(w => w.HardwareType == HardwareType.Network)
                .ToList();

            UpdateHardware();
            Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;

            void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != nameof(Global.Configuration.GsiNetworkDevice))
                {
                    return;
                }
                UpdateHardware();
            }
        }

        private void UpdateHardware()
        {
            hw = _networkDevices.FirstOrDefault(w => w.Name == Global.Configuration.GsiNetworkDevice);
            if (hw is null)
            {
                Global.logger.Error("[HardwareMonitor] Could not find hardware of type Network or hardware monitoring is disabled");
                return;
            }
            _bandwidthUsed = FindSensor(SensorType.Load);
            _uploadSpeed = FindSensor("throughput/7");
            _downloadSpeed = FindSensor("throughput/8");
        }

        public IEnumerable<IHardware> GetNetworkDevices() => _networkDevices;
    }
}