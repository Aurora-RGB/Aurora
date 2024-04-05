using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common;
using Common.Data;
using Common.Devices;

namespace AuroraRgb.Devices;

public sealed class MemorySharedDevice : IDevice
{
    public event EventHandler? Updated;

    private DeviceInformation DeviceInformation { get; set; }

    public string DeviceName => DeviceInformation.DeviceName;
    public string DeviceDetails => DeviceInformation.DeviceDetails;
    public string DeviceUpdatePerformance => DeviceInformation.DeviceUpdatePerformance;
    public bool IsDoingWork => DeviceInformation.IsDoingWork;
    public bool IsInitialized => DeviceInformation.IsInitialized;
    public VariableRegistry RegisteredVariables { get; } = new();

    private readonly MemorySharedStruct<DeviceInformation> _sharedDeviceInfo;
    private readonly MemorySharedArray<DeviceVariable> _deviceVariables;

    public MemorySharedDevice(string memoryName)
    {
        _sharedDeviceInfo = new MemorySharedStruct<DeviceInformation>(memoryName);
        _sharedDeviceInfo.Updated += OnSharedDeviceInfoOnUpdated;
        UpdateInformation();

        _deviceVariables = new MemorySharedArray<DeviceVariable>(memoryName + "-vars");
        _deviceVariables.Updated += DeviceVariablesOnUpdated;
        _deviceVariables.RequestUpdate();
        UpdateVariables();
    }

    private void OnSharedDeviceInfoOnUpdated(object? o, EventArgs eventArgs)
    {
        UpdateInformation();
    }

    private void DeviceVariablesOnUpdated(object? sender, EventArgs e)
    {
        UpdateVariables();
    }

    private void UpdateInformation()
    {
        var deviceInformation = _sharedDeviceInfo.ReadElement();
        DeviceInformation = deviceInformation;
        Updated?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateVariables()
    {
        foreach (var deviceVariable in _deviceVariables)
        {
            Func<byte[]?, object> convert = deviceVariable.ValueType switch
            {
                VariableType.None => _ => new object(),
                VariableType.Boolean => data => BitConverter.ToBoolean(data),
                VariableType.Int => data => BitConverter.ToInt32(data),
                VariableType.Long => data => BitConverter.ToInt64(data),
                VariableType.Float => data => BitConverter.ToSingle(data),
                VariableType.Double => data => BitConverter.ToDouble(data),
                VariableType.Color => data => Color.FromArgb(BitConverter.ToInt32(data)),
                VariableType.DeviceKeys => data => (DeviceKeys)BitConverter.ToInt32(data),
                VariableType.String => _ => string.Empty,
            };

            if (deviceVariable.ValueType == VariableType.String)
            {
                RegisteredVariables.Register(deviceVariable.Name, new VariableRegistryItem(
                    deviceVariable.StringValue,
                    deviceVariable.DefaultStringValue,
                    null,
                    null,
                    deviceVariable.Title,
                    deviceVariable.Remark,
                    (IntVariableDisplay)deviceVariable.Flags
                ));
            }
            else
            {
                RegisteredVariables.Register(deviceVariable.Name, new VariableRegistryItem(
                    convert(deviceVariable.Value),
                    convert(deviceVariable.Default),
                    convert(deviceVariable.Max),
                    convert(deviceVariable.Min),
                    deviceVariable.Title,
                    deviceVariable.Remark,
                    (IntVariableDisplay)deviceVariable.Flags
                ));
            }
        }
    }

    public void RequestUpdate()
    {
        _sharedDeviceInfo.RequestUpdate();
    }

    public IEnumerable<string> GetDevices()
    {
        return DeviceInformation.Devices?.Split(Constants.StringSplit) ?? Enumerable.Empty<string>();
    }

    public void Dispose()
    {
        _sharedDeviceInfo.Dispose();
        _deviceVariables.Dispose();
    }
}