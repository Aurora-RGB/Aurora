using Vortice.XInput;

namespace AuroraRgb.Nodes;

public class Controllers : Node
{
    public readonly ControllerNode Controller1 = new(0);
    public readonly ControllerNode Controller2 = new(1);
    public readonly ControllerNode Controller3 = new(2);
    public readonly ControllerNode Controller4 = new(3);
}

public class ControllerNode(int userIndex) : Node
{
    private State _state;
    private BatteryInformation _battery;

    public bool IsConnected => XInput.GetState(userIndex, out _state);

    public BatteryLevel Battery => XInput.GetBatteryInformation(userIndex, BatteryDeviceType.Gamepad, out _battery) ? _battery.BatteryLevel : BatteryLevel.Empty;

    public byte LeftTrigger => XInput.GetState(userIndex, out _state) ? _state.Gamepad.LeftTrigger : (byte)0;
    public byte RightTrigger => XInput.GetState(userIndex, out _state) ? _state.Gamepad.RightTrigger : (byte)0;

    public short LeftThumbX => XInput.GetState(userIndex, out _state) ? _state.Gamepad.LeftThumbX : (short)0;
    public short LeftThumbY => XInput.GetState(userIndex, out _state) ? _state.Gamepad.LeftThumbY : (short)0;

    public short RightThumbX => XInput.GetState(userIndex, out _state) ? _state.Gamepad.RightThumbX : (short)0;
    public short RightThumbY => XInput.GetState(userIndex, out _state) ? _state.Gamepad.RightThumbY : (short)0;
}