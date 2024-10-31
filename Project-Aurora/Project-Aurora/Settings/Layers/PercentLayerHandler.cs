using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public partial class PercentLayerHandlerProperties : LayerHandlerProperties2Color
{
    private PercentEffectType? _percentType;
    [JsonProperty("_PercentType")]
    public PercentEffectType PercentType
    {
        get => Logic?._percentType ?? _percentType ?? PercentEffectType.Progressive_Gradual;
        set => _percentType = value;
    }

    private double? _blinkThreshold;
    [JsonProperty("_BlinkThreshold")]
    public double BlinkThreshold
    {
        get => Logic?._blinkThreshold ?? _blinkThreshold ?? 0.0;
        set => _blinkThreshold = value;
    }

    private bool? _blinkDirection;
    [JsonProperty("_BlinkDirection")]
    public bool BlinkDirection
    {
        get => Logic?._blinkDirection ?? _blinkDirection ?? false;
        set => _blinkDirection = value;
    }

    private bool? _blinkBackground;
    [JsonProperty("_BlinkBackground")]
    public bool BlinkBackground
    {
        get => Logic?._blinkBackground ?? _blinkBackground ?? false;
        set => _blinkBackground = value;
    }

    private VariablePath? _variablePath;
    [JsonProperty("_VariablePath")]
    public VariablePath VariablePath
    {
        get => Logic?._variablePath ?? _variablePath ?? VariablePath.Empty;
        set => _variablePath = value;
    }

    private VariablePath? _maxVariablePath;
    [JsonProperty("_MaxVariablePath")]
    public VariablePath MaxVariablePath
    {
        get => Logic?._maxVariablePath ?? _maxVariablePath ?? VariablePath.Empty;
        set => _maxVariablePath = value;
    }

    // These two properties work slightly differently to the others. These are special properties that allow for
    // override the value using the overrides system. These are not displayed/directly editable by the user and 
    // will not actually store a value (so should be ignored by the JSON serializers). If these have a value (non
    // null), then they will be used as the value/max value for the percent effect, else the _VariablePath and
    // _MaxVariablePaths will be resolved.
    [JsonIgnore]
    [LogicOverridable("Value")]
    public double? _Value { get; set; }

    [JsonIgnore]
    [LogicOverridable("Max Value")]
    public double? _MaxValue { get; set; }

    public override void Default()
    {
        base.Default();
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();
        SecondaryColor = CommonColorUtils.GenerateRandomColor();
        _percentType = PercentEffectType.Progressive_Gradual;
        _blinkThreshold = 0.0;
        _blinkDirection = false;
        _blinkBackground = false;
    }
}

public class PercentLayerHandler<TProperty>() : LayerHandler<TProperty, BitmapEffectLayer>("PercentLayer")
    where TProperty : PercentLayerHandlerProperties
{
    private double _value;

    private readonly NoRenderLayer NoRenderLayer = new();

    public override EffectLayer Render(IGameState gameState)
    {
        var keySequence = Properties.Sequence;
        EffectLayer layer = keySequence.Type switch
        {
            KeySequenceType.Sequence => NoRenderLayer,
            _ => EffectLayer,
        };
        
        
        if (Invalidated)
        {
            layer.Clear();
            Invalidated = false;
            _value = -1;
        }
        var value = Properties.Logic?._Value ?? gameState.GetNumber(Properties.VariablePath);
        if (MathUtils.NearlyEqual(_value, value, 0.000001))
        {
            return layer;
        }
        _value = value;
            
        var maxvalue = Properties.Logic?._MaxValue ?? gameState.GetNumber(Properties.MaxVariablePath);

        if (keySequence.Type == KeySequenceType.Sequence)
        {
            NoRenderLayer.PercentEffect(Properties.PrimaryColor, Properties.SecondaryColor, keySequence.Keys, value, maxvalue,
                Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection, Properties.BlinkBackground);
            return NoRenderLayer;
        }
        EffectLayer.PercentEffect(Properties.PrimaryColor, Properties.SecondaryColor, keySequence, value, maxvalue,
            Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection, Properties.BlinkBackground);
        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        if (profile != null)
        {
            if (!double.TryParse(Properties.VariablePath.GsiPath, CultureInfo.InvariantCulture, out _) &&
                !string.IsNullOrWhiteSpace(Properties.VariablePath.GsiPath) &&
                !profile.ParameterLookup.IsValidParameter(Properties.VariablePath.GsiPath)
               )
                Properties.VariablePath = VariablePath.Empty;

            if (!double.TryParse(Properties.MaxVariablePath.GsiPath, CultureInfo.InvariantCulture, out _) &&
                !string.IsNullOrWhiteSpace(Properties.MaxVariablePath.GsiPath) &&
                !profile.ParameterLookup.IsValidParameter(Properties.MaxVariablePath.GsiPath)
               )
                Properties.MaxVariablePath = VariablePath.Empty;
        }
        base.SetApplication(profile);
    }
}

public class PercentLayerHandler : PercentLayerHandler<PercentLayerHandlerProperties>
{
    protected override UserControl CreateControl()
    {
        return new Control_PercentLayer(this);
    }
}