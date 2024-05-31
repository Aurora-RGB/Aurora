using System.Drawing;
using AuroraRgb.Settings.Overrides;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public class LayerHandlerProperties2Color<TProperty>(bool assignDefault = false) : LayerHandlerProperties<TProperty>(assignDefault)
    where TProperty : LayerHandlerProperties2Color<TProperty>
{
    private Color? _secondaryColor;

    [LogicOverridable("Secondary Color")]
    public Color? _SecondaryColor
    {
        get => _secondaryColor;
        set
        {
            _secondaryColor = value;
            OnPropertiesChanged(null);
        }
    }

    [JsonIgnore]
    public Color SecondaryColor => Logic?._SecondaryColor ?? _SecondaryColor ?? Color.Empty;

    public override void Default()
    {
        base.Default();
        _SecondaryColor = CommonColorUtils.GenerateRandomColor();
    }
}