using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers
{
    public partial class GradientFillLayerHandlerProperties : LayerHandlerProperties2Color<GradientFillLayerHandlerProperties>
    {
        private LayerEffectConfig _gradientConfig;

        [JsonProperty("_GradientConfig")]
        [LogicOverridable("Gradient")]
        public LayerEffectConfig GradientConfig
        {
            get => Logic?._GradientConfig ?? _gradientConfig;
            set => _gradientConfig = value;
        }

        public bool? _FillEntireKeyboard { get; set; }

        [JsonIgnore]
        public bool FillEntireKeyboard => Logic?._FillEntireKeyboard ?? _FillEntireKeyboard ?? false;

        public GradientFillLayerHandlerProperties()
        { }

        public GradientFillLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _gradientConfig = new LayerEffectConfig(CommonColorUtils.GenerateRandomColor(), CommonColorUtils.GenerateRandomColor()) { AnimationType = AnimationType.None };
            _FillEntireKeyboard = false;
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("SecondaryColor")]
    public class GradientFillLayerHandler : LayerHandler<GradientFillLayerHandlerProperties>
    {
        public GradientFillLayerHandler() : base("GradientFillLayer")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_GradientFillLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            //Get current color
            Properties.GradientConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - Properties.GradientConfig.LastEffectCall) / 1000.0f * 5.0f * Properties.GradientConfig.Speed;
            Properties.GradientConfig.ShiftAmount %= Effects.Canvas.BiggestSize;
            Properties.GradientConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();

            var selectedColor = Properties.GradientConfig.Brush.GetColorSpectrum().GetColorAt(Properties.GradientConfig.ShiftAmount, Effects.Canvas.BiggestSize);

            if (Properties.FillEntireKeyboard)
                EffectLayer.FillOver(selectedColor);
            else
                EffectLayer.Set(Properties.Sequence, selectedColor);

            return EffectLayer;
        }
    }
}
