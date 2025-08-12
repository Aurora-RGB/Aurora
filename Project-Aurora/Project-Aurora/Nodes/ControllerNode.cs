using Vortice.XInput;

namespace AuroraRgb.Nodes;

public class Controllers : Node
{
    public ControllerNode Controller1 { get; } = new(0);
    public ControllerNode Controller2 { get; } = new(1);
    public ControllerNode Controller3 { get; } = new(2);
    public ControllerNode Controller4 { get; } = new(3);
}

public class ControllerNode(int userIndex) : Node
{
    private State _state;
    private BatteryInformation _battery;

    public bool IsConnected => XInput.GetState(userIndex, out _state);

    public BatteryLevel Battery =>
        XInput.GetBatteryInformation(userIndex, BatteryDeviceType.Gamepad, out _battery) ? _battery.BatteryLevel : BatteryLevel.Empty;

    public byte LeftTrigger => XInput.GetState(userIndex, out _state) ? _state.Gamepad.LeftTrigger : (byte)0;
    public byte RightTrigger => XInput.GetState(userIndex, out _state) ? _state.Gamepad.RightTrigger : (byte)0;

    public short LeftThumbX => XInput.GetState(userIndex, out _state) ? _state.Gamepad.LeftThumbX : (short)0;
    public short LeftThumbY => XInput.GetState(userIndex, out _state) ? _state.Gamepad.LeftThumbY : (short)0;

    public short RightThumbX => XInput.GetState(userIndex, out _state) ? _state.Gamepad.RightThumbX : (short)0;
    public short RightThumbY => XInput.GetState(userIndex, out _state) ? _state.Gamepad.RightThumbY : (short)0;
    
    public bool IsLeftThumbPressed => Buttons.HasFlag(GamepadButtons.LeftThumb);
    public bool IsRightThumbPressed => Buttons.HasFlag(GamepadButtons.RightThumb);
    public bool IsLeftShoulderPressed => Buttons.HasFlag(GamepadButtons.LeftShoulder);
    public bool IsRightShoulderPressed => Buttons.HasFlag(GamepadButtons.RightShoulder);
    
    public bool IsAButtonPressed => Buttons.HasFlag(GamepadButtons.A);
    public bool IsBButtonPressed => Buttons.HasFlag(GamepadButtons.B);
    public bool IsXButtonPressed => Buttons.HasFlag(GamepadButtons.X);
    public bool IsYButtonPressed => Buttons.HasFlag(GamepadButtons.Y);
    
    public bool IsDPadUpPressed => Buttons.HasFlag(GamepadButtons.DPadUp);
    public bool IsDPadDownPressed => Buttons.HasFlag(GamepadButtons.DPadDown);
    public bool IsDPadLeftPressed => Buttons.HasFlag(GamepadButtons.DPadLeft);
    public bool IsDPadRightPressed => Buttons.HasFlag(GamepadButtons.DPadRight);
    
    public bool IsStartButtonPressed => Buttons.HasFlag(GamepadButtons.Start);
    public bool IsBackButtonPressed => Buttons.HasFlag(GamepadButtons.Back);
    public bool IsGuideButtonPressed => Buttons.HasFlag(GamepadButtons.Guide);
    
    private GamepadButtons Buttons => XInput.GetState(userIndex, out _state) ? _state.Gamepad.Buttons : GamepadButtons.None;
}