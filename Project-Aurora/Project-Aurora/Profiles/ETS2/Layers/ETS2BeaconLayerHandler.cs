using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.ETS2.GSI;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.ETS2.Layers;

public partial class ETS2BeaconLayerProperties : LayerHandlerProperties<ETS2BeaconLayerProperties>
{
    private ETS2_BeaconStyle? _beaconStyle;

    [JsonProperty("_BeaconStyle")]
    public ETS2_BeaconStyle BeaconStyle
    {
        get => Logic?._BeaconStyle ?? _beaconStyle ?? ETS2_BeaconStyle.Simple_Flash;
        set => _beaconStyle = value;
    }

    private float? _speed;

    [JsonProperty("_Speed")]
    public float Speed
    {
        get => Logic?._Speed ?? _speed ?? 1;
        set => _speed = value;
    }

    public ETS2BeaconLayerProperties()
    {
    }

    public ETS2BeaconLayerProperties(float? speed)
    {
        _speed = speed;
    }
    public ETS2BeaconLayerProperties(float? speed, bool assignDefault = false) : base(assignDefault)
    {
        _speed = speed;
    }

    public override void Default() {
        base.Default();

        _Sequence = new KeySequence(new[] { DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8 });
        _PrimaryColor = Color.FromArgb(255, 128, 0);
        _beaconStyle = ETS2_BeaconStyle.Fancy_Flash;
        _speed = 1f;
    }
}

public class ETS2BeaconLayerHandler : LayerHandler<ETS2BeaconLayerProperties> {

    private int _frame;

    protected override UserControl CreateControl() {
        return new Control_ETS2BeaconLayer(this);
    }

    /// <summary>Multiplies the Primary Color's alpha by this value and returns it.</summary>
    private Color PrimaryColorAlpha(double a) {
        int alpha = CommonColorUtils.ColorByteMultiplication(Properties.PrimaryColor.A, a);
        return Color.FromArgb(alpha, Properties.PrimaryColor);
    }

    public override EffectLayer Render(IGameState gamestate) {
        var layer = new EffectLayer("ETS2 Beacon Layer");

        if (gamestate is GameState_ETS2 { Truck.lightsBeaconOn: true }) {
            switch (Properties.BeaconStyle) {
                // Fades all assigned lights in and out together
                case ETS2_BeaconStyle.Simple_Flash:
                    var multiplier = Math.Pow(Math.Sin(_frame * Properties.Speed * Math.PI / 10), 2);
                    layer.Set(Properties.Sequence, PrimaryColorAlpha(multiplier));
                    _frame = (_frame + 1) % (int)(10 / Properties.Speed);
                    break;

                // Flashes lights in a pattern similar to the ETS2 LED beacons
                // Pattern: ###------###------#-#-#----#-#-#----#-#-#---- [2x###------, 3x#-#-#----] (# = on, - = off)
                case ETS2_BeaconStyle.Fancy_Flash:
                    // To get the keyframe number we divide frame by 2 because it was too fast otherwise
                    var m10 = (_frame / 2) % 9; // Mod 10 of the keyframe (10 is the size of a group)
                    var on = (_frame / 2) < 18
                        ? m10 < 4 // When in one of the first two groups, light up for the first 4 keyframes of that group
                        : m10 is 0 or 2 or 4; // When in the last 3 groups, light up if the keyframe is 0th, 2nd or 4th of that group

                    if (on)
                        layer.Set(Properties.Sequence, Properties.PrimaryColor);

                    _frame = (_frame + 1) % 90; // 90 because there are 9 keyframes per group, 5 groups and each keyframe = 2 real frames (9 * 5 * 2)
                    break;

                // Sets half the sequence on and half off, then swaps. If odd number of keys, first half will be bigger
                case ETS2_BeaconStyle.Half_Alternating:
                    List<DeviceKeys> half;
                    if (_frame < 5)
                        // First half
                        half = Properties.Sequence.Keys.ToList().GetRange(0, (int)Math.Ceiling((double)Properties.Sequence.Keys.Count / 2));
                    else
                        // Second half
                        half = Properties.Sequence.Keys.ToList().GetRange((int)Math.Ceiling((double)Properties.Sequence.Keys.Count / 2), (int)Math.Ceiling((double)Properties.Sequence.Keys.Count / 2));

                    layer.Set(half.ToArray(), Properties.PrimaryColor);

                    _frame = (_frame + 1) % 10;
                    break;
                    
                // The "on" key goes up and down the sequence
                case ETS2_BeaconStyle.Side_To_Side:
                    var keyCount = Properties.Sequence.Keys.Count;

                    var light = Math.Abs(((_frame/2 + 1) % (keyCount * 2 - 2)) - keyCount + 2);
                    var prevLight = Math.Abs(_frame/2 - keyCount + 2);
                    layer.Set(Properties.Sequence.Keys[light], Properties.PrimaryColor);
                    layer.Set(Properties.Sequence.Keys[prevLight], PrimaryColorAlpha(.5));

                    _frame = (_frame + 1) % ((keyCount - 1) * 4); // *4 because we want the pattern to go up and down (*2), and also each keyframe should take 2 real frames
                    break;
            }

        }  else // When the beacon is off, reset the frame counter so that the animation plays from the start
            _frame = 0;

        return layer;
    }
}