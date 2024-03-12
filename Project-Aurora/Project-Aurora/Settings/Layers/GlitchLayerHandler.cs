using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers
{
    
    public class GlitchLayerHandlerProperties<TProperty> : LayerHandlerProperties2Color<TProperty> where TProperty : GlitchLayerHandlerProperties<TProperty>
    {
        [JsonIgnore] private double? _updateInterval;

        [JsonProperty("_UpdateInterval")]
        public double UpdateInterval
        {
            get => Logic._updateInterval ?? _updateInterval ?? 1.0;
            set
            {
                _updateInterval = value;
                OnPropertiesChanged(this);
            }
        }

        [JsonIgnore]
        private bool? _allowTransparency;

        [JsonProperty("_AllowTransparency")]
        public bool AllowTransparency
        {
            get => Logic._allowTransparency ?? _allowTransparency ?? false;
            set
            {
                _allowTransparency = value;
                OnPropertiesChanged(this);
            }
        }

        public GlitchLayerHandlerProperties()
        { }

        public GlitchLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _updateInterval = 1.0;
            _allowTransparency = false;
            _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
        }
    }

    public class GlitchLayerHandlerProperties : GlitchLayerHandlerProperties<GlitchLayerHandlerProperties>
    {
        public GlitchLayerHandlerProperties()
        { }

        public GlitchLayerHandlerProperties(bool empty = false) : base(empty) { }
    }

    public class GlitchLayerHandler<TProperty> : LayerHandler<TProperty> where TProperty : GlitchLayerHandlerProperties<TProperty>
    {
        private readonly Random _randomizer = new();

        private readonly Dictionary<DeviceKeys, Color> _glitchColors = new();

        private long _previousTime;
        private long _currentTime;

        public GlitchLayerHandler() : base("Glitch Layer")
        {
        }

        public override EffectLayer Render(IGameState state)
        {
            _currentTime = Time.GetMillisecondsSinceEpoch();
            if (!(_previousTime + Properties.UpdateInterval * 1000L <= _currentTime)) return EffectLayer;
            _previousTime = _currentTime;

            var keys = Properties.Sequence.Type == KeySequenceType.FreeForm ? Enum.GetValues(typeof(DeviceKeys)) : Properties.Sequence.Keys.ToArray();
            foreach (DeviceKeys key in keys)
            {
                var clr = Properties.AllowTransparency ? _randomizer.Next() % 2 == 0 ? Color.Transparent : CommonColorUtils.GenerateRandomColor() : CommonColorUtils.GenerateRandomColor();

                _glitchColors[key] = clr;
            }

            foreach (var kvp in _glitchColors)
            {
                EffectLayer.Set(kvp.Key, kvp.Value);
            }
            EffectLayer.OnlyInclude(Properties.Sequence);
            return EffectLayer;

        }

        protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
        {
            base.PropertiesChanged(sender, args);
            _glitchColors.Clear();
            EffectLayer.Invalidate();
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    public class GlitchLayerHandler : GlitchLayerHandler<GlitchLayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_GlitchLayer(this);
        }
    }
}
