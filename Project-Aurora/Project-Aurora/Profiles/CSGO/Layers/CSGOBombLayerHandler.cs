using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public class CSGOBombLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBombLayerHandlerProperties>
{
    private Color? _flashColor;

    [JsonProperty("_FlashColor")]
    public Color FlashColor
    {
        get => Logic?._flashColor ?? _flashColor ?? Color.Empty;
        set => _flashColor = value;
    }

    private Color? _primedColor;

    [JsonProperty("_PrimedColor")]
    public Color PrimedColor
    {
        get => Logic?._primedColor ?? _primedColor ?? Color.Empty;
        set => _primedColor = value;
    }

    private bool? _gradualEffect;

    [JsonProperty("_GradualEffect")]
    public bool GradualEffect
    {
        get => Logic?._gradualEffect ?? _gradualEffect ?? false;
        set => _gradualEffect = value;
    }

    public CSGOBombLayerHandlerProperties()
    { }

    public CSGOBombLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();

        _Sequence = new KeySequence(
        [
            DeviceKeys.NUM_LOCK, DeviceKeys.NUM_SLASH, DeviceKeys.NUM_ASTERISK, DeviceKeys.NUM_MINUS, DeviceKeys.NUM_SEVEN,
            DeviceKeys.NUM_EIGHT, DeviceKeys.NUM_NINE, DeviceKeys.NUM_PLUS, DeviceKeys.NUM_FOUR, DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX,
            DeviceKeys.NUM_ONE, DeviceKeys.NUM_TWO, DeviceKeys.NUM_THREE, DeviceKeys.NUM_ZERO, DeviceKeys.NUM_PERIOD, DeviceKeys.NUM_ENTER,
            DeviceKeys.CL1, DeviceKeys.CL2, DeviceKeys.CL3, DeviceKeys.CL4, DeviceKeys.CL5,
        ]);
        _flashColor = Color.FromArgb(255, 0, 0);
        _primedColor = Color.FromArgb(0, 255, 0);
        _gradualEffect = true;
    }
}

public class CSGOBombLayerHandler() : LayerHandler<CSGOBombLayerHandlerProperties>("CSGO - Bomb Effect")
{
    private readonly Stopwatch _bombTimer = new();

    private bool _bombFlash;
    private int _bombFlashCount;
    private long _bombFlashTime;
    private long _bombFlashEdat;

    protected override UserControl CreateControl()
    {
        return new Control_CSGOBombLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameState_CSGO csgostate) return EffectLayer.EmptyLayer;

        if (csgostate.Round.Bomb != BombState.Planted)
        {
            if (!_bombTimer.IsRunning) return EffectLayer.EmptyLayer;
            Reset();

            return EffectLayer.EmptyLayer;
        }
        if (!_bombTimer.IsRunning)
        {
            _bombTimer.Restart();
        }

        double flashAmount;
        var isCritical = false;

        switch (_bombTimer.ElapsedMilliseconds)
        {
            case < 35000:
            {
                if (_bombTimer.ElapsedMilliseconds >= _bombFlashTime)
                {
                    _bombFlash = true;
                    _bombFlashEdat = _bombTimer.ElapsedMilliseconds;
                    _bombFlashTime = _bombTimer.ElapsedMilliseconds + (1000 - _bombFlashCount++ * 13);
                }

                if (_bombTimer.ElapsedMilliseconds < _bombFlashEdat || _bombTimer.ElapsedMilliseconds > _bombFlashEdat + 220)
                    flashAmount = 0.0;
                else
                    flashAmount = Math.Pow(Math.Sin((_bombTimer.ElapsedMilliseconds - _bombFlashEdat) / 80.0 + 0.25), 2.0);
                break;
            }
            case >= 35000:
                isCritical = true;
                flashAmount = _bombTimer.ElapsedMilliseconds / 40000.0;
                break;
        }

        if (!isCritical)
        {
            if (flashAmount <= 0.05 && _bombFlash)
                _bombFlash = false;

            if (!_bombFlash)
                flashAmount = 0.0;
        }

        if (!Properties.GradualEffect)
            flashAmount = Math.Round(flashAmount);

        if (flashAmount < 0.01)
        {
            return EffectLayer.EmptyLayer;
        }

        var bombColor = ColorUtils.MultiplyColorByScalar(isCritical ? Properties.PrimedColor : Properties.FlashColor, Math.Min(flashAmount, 1.0));

        EffectLayer.Set(Properties.Sequence, bombColor);

        return EffectLayer;
    }

    private void Reset()
    {
        _bombTimer.Stop();
        _bombFlash = false;
        _bombFlashCount = 0;
        _bombFlashTime = 0;
        _bombFlashEdat = 0;
    }
}