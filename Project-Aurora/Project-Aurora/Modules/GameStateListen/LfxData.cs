using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;

namespace AuroraRgb.Modules.GameStateListen;

[JsonSerializable(typeof(LfxData))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public partial class LfxJsonSourceContext : JsonSerializerContext;

public static class LfxState
{
    /// <summary>
    /// Returns whether or not the wrapper is connected through IPC
    /// </summary>
    public static bool IsWrapperConnected { get; set; }
    /// <summary>
    /// Returns the process of the wrapped connection
    /// </summary>
    public static string WrappedProcess { get; set; } = "";
    
    public static LfxData LastData { get; private set; } = LfxData.Empty;
    public static readonly int[] Bitmap = new int[126];
    public static Dictionary<DeviceKeys, Color> ExtraKeys { get; } = new();
    public static Color LastFillColor { get; private set; } = Color.Transparent;
    public static EntireEffect? CurrentEffect { get; private set; }

    public static void SetGameState(LfxData ngwState)
    {
        LastData = ngwState;
        switch (ngwState.Command)
        {
            //LightFX
            case "LFX_GetNumDevices":
            case "LFX_GetNumLights":
            {
                break;
            }
            case "LFX_Light":
            case "LFX_SetLightColor":
            {
                //Retain previous lighting
                var fillColorInt = CommonColorUtils.GetIntFromColor(LastFillColor);

                for (var i = 0; i < Bitmap.Length; i++)
                    Bitmap[i] = fillColorInt;

                foreach (var extraKey in ExtraKeys.Keys.ToArray())
                    ExtraKeys[extraKey] = LastFillColor;
                break;
            }
            case "LFX_Update":
            {
                var newFill = ngwState.CommandData.PrimaryColor;

                if (LastFillColor != newFill)
                {
                    LastFillColor = newFill;

                    for (var i = 0; i < Bitmap.Length; i++)
                    {
                        Bitmap[i] = (ngwState.CommandData.AlphaStart << 24) | (ngwState.CommandData.RedStart << 16) | (ngwState.CommandData.GreenStart << 8) | ngwState.CommandData.BlueStart;
                    }
                }

                foreach (var extraKey in ExtraKeys.Keys.ToArray())
                    ExtraKeys[extraKey] = newFill;
                break;
            }
            case "LFX_SetLightActionColor":
            case "LFX_ActionColor":
            {
                var primary = Color.Transparent;
                var secondary = ngwState.CommandData.PrimaryColor;

                if (CurrentEffect != null)
                    primary = CurrentEffect.GetCurrentColor(Time.GetMillisecondsSinceEpoch() - CurrentEffect.TimeStarted);

                CurrentEffect = ngwState.CommandData.EffectType switch
                {
                    "LFX_ACTION_COLOR" => new LFX_Color(primary),
                    "LFX_ACTION_PULSE" => new LFX_Pulse(primary, secondary, ngwState.CommandData.Duration),
                    "LFX_ACTION_MORPH" => new LFX_Morph(primary, secondary, ngwState.CommandData.Duration),
                    _ => null
                };

                break;
            }
            case "LFX_SetLightActionColorEx":
            case "LFX_ActionColorEx":
            {
                var primary = ngwState.CommandData.PrimaryColor;
                var secondary = ngwState.CommandData.SecondaryColor;

                CurrentEffect = ngwState.CommandData.EffectType switch
                {
                    "LFX_ACTION_COLOR" => new LFX_Color(primary),
                    "LFX_ACTION_PULSE" => new LFX_Pulse(primary, secondary, ngwState.CommandData.Duration),
                    "LFX_ACTION_MORPH" => new LFX_Morph(primary, secondary, ngwState.CommandData.Duration),
                    _ => null
                };

                break;
            }
            case "LFX_Reset":
                CurrentEffect = null;
                break;
            default:
                Global.logger.Information("Unknown Wrapper Command: {Command} Data: {Data}", ngwState.Command, ngwState.CommandData);
                break;
        }
    }
}

public class EntireEffect
{
    protected Color Color;
    protected readonly long Duration;
    public long Interval;
    public readonly long TimeStarted;

    protected EntireEffect(Color color, long duration, long interval)
    {
        Color = color;
        Duration = duration;
        Interval = interval;
        TimeStarted = Time.GetMillisecondsSinceEpoch();
    }

    public virtual ref readonly Color GetCurrentColor(long time)
    {
        return ref Color;
    }

    public void SetEffect(EffectLayer layer, long time)
    {
        layer.FillOver(in GetCurrentColor(time));
    }
}

public class LfxData
{
    public static LfxData Empty { get; } = new();

    public string Command { get; set; } = string.Empty;
    public LfxCommandData CommandData { get; set; } = new();
    public LfxProviderData Provider { get; set; } = new();
}

public class LfxCommandData
{
    public int RedStart { get; set; }
    public int GreenStart { get; set; }
    public int BlueStart { get; set; }
    public int AlphaStart { get; set; } = 255;

    public int RedEnd { get; set; }
    public int GreenEnd { get; set; }
    public int BlueEnd { get; set; }
    public int AlphaEnd { get; set; } = 255;

    public string LocationMask { get; set; } = string.Empty;

    public int Duration { get; set; }

    public string EffectType { get; set; } = string.Empty;
    
    public Color PrimaryColor => Color.FromArgb(AlphaStart, RedStart, GreenStart, BlueStart);
    public Color SecondaryColor => Color.FromArgb(AlphaEnd, RedEnd, GreenEnd, BlueEnd);
}

public class LfxProviderData
{
    public string Name { get; set; } = string.Empty;
}