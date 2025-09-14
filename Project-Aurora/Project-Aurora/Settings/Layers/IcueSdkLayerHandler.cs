using System;
using System.Collections.Generic;
using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Icue;
using AuroraRgb.Profiles;
using Common.Devices;
using Common.Utils;
using System.Windows.Controls;
using AuroraRgb.Settings.Layers.Controls;

namespace AuroraRgb.Settings.Layers;

public partial class IcueSdkLayerHandlerProperties : LayerHandlerProperties
{
    private Dictionary<DeviceKeys, DeviceKeys>? _keyCloneMap;

    public Dictionary<DeviceKeys, DeviceKeys> KeyCloneMap
    {
        get => Logic?._keyCloneMap ?? (_keyCloneMap ??= new Dictionary<DeviceKeys, DeviceKeys>());
        set => _keyCloneMap = value;
    }

    public override void Default()
    {
        base.Default();
        _keyCloneMap = new Dictionary<DeviceKeys, DeviceKeys>();
    }
}

[LayerHandlerMeta(Name = "Corsair iCUE", IsDefault = true)]
public sealed class IcueSdkLayerHandler : LayerHandler<IcueSdkLayerHandlerProperties>
{
    public IcueSdkLayerHandler() : base("Corsair iCUE")
    {
        IcueModule.AuroraIcueServer.Sdk.ColorsUpdated += SdkListenerOnColorsUpdated;
    }

    protected override UserControl CreateControl() => new Control_IcueLayer(this);

    public override EffectLayer Render(IGameState gameState)
    {
        if (!IcueModule.AuroraIcueServer.Sdk.IsGameConnected)
        {
            return EmptyLayer.Instance;
        }

        if (!Invalidated)
        {
            return EffectLayer;
        }

        // Build a temporary map from DeviceKeys to Colors based on current iCUE colors
        var deviceColorMap = new Dictionary<DeviceKeys, Color>();
        foreach (var (icueLedId, icueColor) in IcueModule.AuroraIcueServer.Sdk.Colors)
        {
            if (!IcueAuroraKeyMapping.KeyMapping.TryGetValue(icueLedId, out var keyId))
                continue;
            var color = CommonColorUtils.FastColor(
                icueColor.R,
                icueColor.G,
                icueColor.B
            );
            deviceColorMap[keyId] = color;
            EffectLayer.Set(keyId, in color);
        }

        // Apply KeyCloneMap similar to LogitechLayerHandler
        foreach (var (target, source) in Properties.KeyCloneMap)
        {
            if (deviceColorMap.TryGetValue(source, out var clr))
            {
                EffectLayer.Set(target, in clr);
            }
        }

        Invalidated = false;
        return EffectLayer;
    }

    public override void Dispose()
    {
        base.Dispose();

        IcueModule.AuroraIcueServer.Sdk.ColorsUpdated -= SdkListenerOnColorsUpdated;
    }

    private void SdkListenerOnColorsUpdated(object? sender, EventArgs e)
    {
        Invalidated = true;
    }
}