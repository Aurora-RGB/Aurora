using System;
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

public partial class PD2StatesLayerHandlerProperties : LayerHandlerProperties2Color<PD2StatesLayerHandlerProperties>
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

    public PD2StatesLayerHandlerProperties()
    { }

    public PD2StatesLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

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

public class PD2StatesLayerHandler : LayerHandler<PD2StatesLayerHandlerProperties>
{
    protected override UserControl CreateControl()
    {
        return new Control_PD2StatesLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        var statesLayer = new EffectLayer("Payday 2 - States");

        if (state is not GameState_PD2 pd2State) return statesLayer;

        if (pd2State.Game.State != GameStates.Ingame) return statesLayer;
        switch (pd2State.LocalPlayer.State)
        {
            case PlayerState.Incapacitated or PlayerState.Bleed_out or PlayerState.Fatal:
            {
                var incapAlpha = (int)(pd2State.LocalPlayer.DownTime > 10 ? 0 : 255 * (1.0D - (double)pd2State.LocalPlayer.DownTime / 10.0D));

                if (incapAlpha > 255)
                    incapAlpha = 255;
                else if (incapAlpha < 0)
                    incapAlpha = 0;

                var incapColor = Color.FromArgb(incapAlpha, Properties.DownedColor);

                statesLayer.FillOver(incapColor);
                statesLayer.Set(DeviceKeys.Peripheral, incapColor);
                break;
            }
            case PlayerState.Arrested:
                statesLayer.FillOver(Properties.ArrestedColor);
                statesLayer.Set(DeviceKeys.Peripheral, Properties.ArrestedColor);
                break;
        }

        if (!pd2State.LocalPlayer.SwanSong || !Properties.ShowSwanSong) return statesLayer;
        var blendPercent = Math.Pow(Math.Cos((Time.GetMillisecondsSinceEpoch() % 1300L) / 1300.0D * Properties.SwanSongSpeedMultiplier * 2.0D * Math.PI), 2.0D);

        var swansongColor = ColorUtils.MultiplyColorByScalar(Properties.SwanSongColor, blendPercent);

        var swansongLayer = new EffectLayer("Payday 2 - Swansong", swansongColor);
        swansongLayer.Set(DeviceKeys.Peripheral, swansongColor);

        statesLayer += swansongLayer;

        return statesLayer;
    }
}