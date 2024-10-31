using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Payday_2.GSI;
using AuroraRgb.Profiles.Payday_2.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Payday_2.Layers;

public partial class PD2StatesLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _downedColor;

    [JsonProperty("_DownedColor")]
    public Color DownedColor
    {
        get => Logic?._DownedColor ?? _downedColor ?? Color.Empty;
        set => _downedColor = value;
    }

    private Color? _arrestedColor;

    [JsonProperty("_ArrestedColor")]
    public Color ArrestedColor
    {
        get => Logic?._ArrestedColor ?? _arrestedColor ?? Color.Empty;
        set => _arrestedColor = value;
    }

    private Color? _swanSongColor;

    [JsonProperty("_SwanSongColor")]
    public Color SwanSongColor
    {
        get => Logic?._SwanSongColor ?? _swanSongColor ?? Color.Empty;
        set => _swanSongColor = value;
    }

    private bool? _showSwanSong;

    [JsonProperty("_ShowSwanSong")]
    public bool ShowSwanSong
    {
        get => Logic?._ShowSwanSong ?? _showSwanSong ?? false;
        set => _showSwanSong = value;
    }

    private float? _swanSongSpeedMultiplier;

    [JsonProperty("_SwanSongSpeedMultiplier")]
    public float SwanSongSpeedMultiplier
    {
        get => Logic?._SwanSongSpeedMultiplier ?? _swanSongSpeedMultiplier ?? 0.0F;
        set => _swanSongSpeedMultiplier = value;
    }

    public override void Default()
    {
        base.Default();

        _downedColor = Color.White;
        _arrestedColor = Color.DarkRed;
        _showSwanSong = true;
        _swanSongColor = Color.FromArgb(158, 205, 255);
        _swanSongSpeedMultiplier = 1.0f;
    }

}

public class PD2StatesLayerHandler() : LayerHandler<PD2StatesLayerHandlerProperties>("PD2StatesLayerHandler")
{
    private readonly EffectLayer _swansongLayer = new BitmapEffectLayer("Payday 2 - Swansong", true);

    protected override UserControl CreateControl()
    {
        return new Control_PD2StatesLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameState_PD2 pd2State) return EmptyLayer.Instance;

        if (pd2State.Game.State != GameStates.Ingame) return EmptyLayer.Instance;
        switch (pd2State.LocalPlayer.State)
        {
            case PlayerState.Incapacitated or PlayerState.Bleed_out or PlayerState.Fatal:
            {
                var incapAlpha = (int)(pd2State.LocalPlayer.DownTime > 10 ? 0 : 255 * (1.0D - pd2State.LocalPlayer.DownTime / 10.0D));

                if (incapAlpha > 255)
                    incapAlpha = 255;
                else if (incapAlpha < 0)
                    incapAlpha = 0;

                var incapColor = Color.FromArgb(incapAlpha, Properties.DownedColor);

                EffectLayer.FillOver(in incapColor);
                EffectLayer.Set(DeviceKeys.Peripheral, in incapColor);
                break;
            }
            case PlayerState.Arrested:
                EffectLayer.FillOver(Properties.ArrestedColor);
                EffectLayer.Set(DeviceKeys.Peripheral, Properties.ArrestedColor);
                break;
        }

        if (!pd2State.LocalPlayer.SwanSong || !Properties.ShowSwanSong) return EffectLayer;
        var blendPercent = Math.Pow(Math.Cos(Time.GetMillisecondsSinceEpoch() % 1300L / 1300.0D * Properties.SwanSongSpeedMultiplier * 2.0D * Math.PI), 2.0D);

        var swansongColor = ColorUtils.MultiplyColorByScalar(Properties.SwanSongColor, blendPercent);

        _swansongLayer.Set(DeviceKeys.Peripheral, swansongColor);

        EffectLayer.Add(_swansongLayer);
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        
        _swansongLayer.Set(DeviceKeys.Peripheral, Properties.SwanSongColor);
    }
}