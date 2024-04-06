using AuroraRgb.Controls;

namespace AuroraRgb.Settings.Layers.Controls;

public partial class Control_TimerLayer
{
    public Control_TimerLayer(TimerLayerHandler context) {
        InitializeComponent();
        DataContext = context;

        triggerKeyList.Keybinds = context.Properties.TriggerKeys;
    }

    private void triggerKeyList_KeybindsChanged(object? sender) {
        if (sender is KeyBindList kbl && DataContext is TimerLayerHandler timerLayerHandler)
            timerLayerHandler.Properties.TriggerKeys = kbl.Keybinds;
    }
}