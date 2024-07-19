using System;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Payday_2.GSI;
using AuroraRgb.Profiles.Payday_2.GSI.Nodes;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Payday_2.Layers;

public partial class PD2BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<PD2BackgroundLayerHandlerProperties>
{
    private Color? _ambientColor;

    [JsonProperty("_AmbientColor")]
    public Color AmbientColor => Logic?._AmbientColor ?? _ambientColor ?? Color.Empty;

    private Color? _assaultColor;

    [JsonProperty("_AssaultColor")]
    public Color AssaultColor => Logic?._AssaultColor ?? _assaultColor ?? Color.Empty;

    private Color? _wintersColor;

    [JsonProperty("_WintersColor")]
    public Color WintersColor => Logic?._WintersColor ?? _wintersColor ?? Color.Empty;

    private float? _assaultSpeedMultiplier;

    [JsonProperty("_AssaultSpeedMultiplier")]
    public float AssaultSpeedMultiplier => Logic?._AssaultSpeedMultiplier ?? _assaultSpeedMultiplier ?? 0.0F;

    private Color? _assaultFadeColor;

    [JsonProperty("_AssaultFadeColor")]
    public Color AssaultFadeColor => Logic?._AssaultFadeColor ?? _assaultFadeColor ?? Color.Empty;

    private bool? _assaultAnimationEnabled;

    [JsonProperty("_AssaultAnimationEnabled")]
    public bool AssaultAnimationEnabled => Logic?._AssaultAnimationEnabled ?? _assaultAnimationEnabled ?? false;

    private Color? _lowSuspicionColor;

    [JsonProperty("_LowSuspicionColor")]
    public Color LowSuspicionColor => Logic?._LowSuspicionColor ?? _lowSuspicionColor ?? Color.Empty;

    private Color? _mediumSuspicionColor;

    [JsonProperty("_MediumSuspicionColor")]
    public Color MediumSuspicionColor => Logic?._MediumSuspicionColor ?? _mediumSuspicionColor ?? Color.Empty;

    private Color? _highSuspicionColor;

    [JsonProperty("_HighSuspicionColor")]
    public Color HighSuspicionColor => Logic?._HighSuspicionColor ?? _highSuspicionColor ?? Color.Empty;

    private bool? _showSuspicion;

    [JsonProperty("_ShowSuspicion")]
    public bool ShowSuspicion => Logic?._ShowSuspicion ?? _showSuspicion ?? false;

    private PercentEffectType? _suspicionEffectType;

    [JsonProperty("_SuspicionEffectType")]
    public PercentEffectType SuspicionEffectType => Logic?._SuspicionEffectType ?? _suspicionEffectType ?? PercentEffectType.AllAtOnce;

    private bool? _peripheralUse;

    [JsonProperty("_PeripheralUse")]
    public bool PeripheralUse => Logic?._PeripheralUse ?? _peripheralUse ?? false;


    public PD2BackgroundLayerHandlerProperties() : base() { }

    public PD2BackgroundLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();

        _ambientColor = Color.FromArgb(158, 205, 255);
        _assaultColor = Color.FromArgb(158, 205, 255);
        _wintersColor = Color.FromArgb(221, 99, 33);
        _assaultSpeedMultiplier = 1.0f;
        _assaultFadeColor = Color.FromArgb(255, 255, 255);
        _assaultAnimationEnabled = true;
        _lowSuspicionColor = Color.FromArgb(0, 0, 0, 255);
        _mediumSuspicionColor = Color.FromArgb(255, 0, 0, 255);
        _highSuspicionColor = Color.FromArgb(255, 255, 0, 0);
        _showSuspicion = true;
        _suspicionEffectType = PercentEffectType.Progressive;
        _peripheralUse = true;
    }
}

public class PD2BackgroundLayerHandler : LayerHandler<PD2BackgroundLayerHandlerProperties>
{
    private float _noReturnFlashamount = 1.0f;
    private float _noReturnTimeleft;

    protected override UserControl CreateControl()
    {
        return new Control_PD2BackgroundLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        var bgLayer = new EffectLayer("Payday 2 - Background");

        if (state is not GameState_PD2 pd2) return bgLayer;
        var bgColor = Properties.AmbientColor;

        var currenttime = Time.GetMillisecondsSinceEpoch();

        if (pd2.Level.Phase is LevelPhase.Assault or LevelPhase.Winters && pd2.Game.State == GameStates.Ingame)
        {
            if (pd2.Level.Phase == LevelPhase.Assault)
                bgColor = Properties.AssaultColor;
            else if (pd2.Level.Phase == LevelPhase.Winters)
                bgColor = Properties.WintersColor;

            var blendPercent = Math.Pow(Math.Sin(((currenttime % 1300L) / 1300.0D) * Properties.AssaultSpeedMultiplier * 2.0D * Math.PI), 2.0D);

            bgColor = ColorUtils.BlendColors(Properties.AssaultFadeColor, bgColor, blendPercent);

            if (Properties.AssaultAnimationEnabled)
            {

                var effectContours = Color.FromArgb(200, Color.Black);
                var animationStageYoffset = 20.0f;
                var animationRepeatKeyframes = 250.0f; //Effects.canvas_width * 2.0f;

                /* Effect visual:

                    / /  ----  / /

                    */

                /*
                     * !!!NOTE: TO BE REWORKED INTO ANIMATIONS!!!

                    EffectColorFunction line1_col_func = new EffectColorFunction(
                        new EffectLine(-1f, Effects.canvas_width + assault_yoffset + animation_stage_yoffset),
                        new ColorSpectrum(effect_contours),
                        2);

                    EffectColorFunction line2_col_func = new EffectColorFunction(
                        new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 9.0f + animation_stage_yoffset),
                        new ColorSpectrum(effect_contours),
                        2);

                    EffectColorFunction line3_col_func = new EffectColorFunction(
                        new EffectLine(new EffectPoint(Effects.canvas_width + assault_yoffset + 17.0f + animation_stage_yoffset, Effects.canvas_height / 2.0f), new EffectPoint(Effects.canvas_width + assault_yoffset + 34.0f + animation_stage_yoffset, Effects.canvas_height / 2.0f), true),
                        new ColorSpectrum(effect_contours),
                        6);

                    EffectColorFunction line4_col_func = new EffectColorFunction(
                        new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 52.0f + animation_stage_yoffset),
                        new ColorSpectrum(effect_contours),
                        2);

                    EffectColorFunction line5_col_func = new EffectColorFunction(
                        new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 61.0f + animation_stage_yoffset),
                        new ColorSpectrum(effect_contours),
                        2);

                    assault_yoffset -= 0.50f;
                    assault_yoffset = assault_yoffset % animation_repeat_keyframes;

                    bg_layer.AddPostFunction(line1_col_func);
                    bg_layer.AddPostFunction(line2_col_func);
                    //bg_layer.AddPostFunction(line3_col_func);
                    bg_layer.AddPostFunction(line4_col_func);
                    bg_layer.AddPostFunction(line5_col_func);

                    */
            }

            bgLayer.FillOver(bgColor);

            if (Properties.PeripheralUse)
                bgLayer.Set(DeviceKeys.Peripheral, bgColor);
        }
        else if (pd2.Level.Phase == LevelPhase.Stealth && pd2.Game.State == GameStates.Ingame)
        {
            if (Properties.ShowSuspicion)
            {
                var percentSuspicious = ((double)pd2.Players.LocalPlayer.SuspicionAmount / (double)1.0);

                var suspicionSpec = new ColorSpectrum(Properties.LowSuspicionColor, Properties.HighSuspicionColor);
                suspicionSpec.SetColorAt(0.5f, Properties.MediumSuspicionColor);

                var suspicionSequence = new KeySequence(new FreeFormObject(0, 0, 1.0f / (Effects.Canvas.EditorToCanvasWidth / Effects.Canvas.Width), 1.0f / (Effects.Canvas.EditorToCanvasHeight / Effects.Canvas.Height)));

                bgLayer.PercentEffect(suspicionSpec, suspicionSequence, percentSuspicious, 1.0D, Properties.SuspicionEffectType);

                if (Properties.PeripheralUse)
                    bgLayer.Set(DeviceKeys.Peripheral, suspicionSpec.GetColorAt((float)percentSuspicious));
            }
        }
        else if (pd2.Level.Phase == LevelPhase.Point_of_no_return && pd2.Game.State == GameStates.Ingame)
        {
            var noReturnSpec = new ColorSpectrum(Color.Red, Color.Yellow);
            if (pd2.Level.NoReturnTime != _noReturnTimeleft)
            {
                _noReturnTimeleft = pd2.Level.NoReturnTime;
                _noReturnFlashamount = 1.0f;
            }

            var noReturnColor = noReturnSpec.GetColorAt(_noReturnFlashamount);
            _noReturnFlashamount -= 0.05f;

            if (_noReturnFlashamount < 0.0f)
                _noReturnFlashamount = 0.0f;

            bgLayer.FillOver(noReturnColor);

            if (Properties.PeripheralUse)
                bgLayer.Set(DeviceKeys.Peripheral, noReturnColor);
        }
        else
        {
            bgLayer.FillOver(bgColor);

            if (Properties.PeripheralUse)
                bgLayer.Set(DeviceKeys.Peripheral, bgColor);
        }

        return bgLayer;
    }
}