using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.ETS2.GSI;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.ETS2.Layers {
    public partial class Ets2BlinkerLayerHandlerProperties : LayerHandlerProperties2Color<Ets2BlinkerLayerHandlerProperties> {
        private Color? _blinkerOffColor;

        [JsonProperty("_BlinkerOffColor")]
        public Color BlinkerOffColor
        {
            get => Logic?._BlinkerOffColor ?? _blinkerOffColor ?? Color.Empty;
            set => _blinkerOffColor = value;
        }

        private Color? _blinkerOnColor;
        [JsonProperty("_BlinkerOnColor")]
        public Color BlinkerOnColor
        {
            get => Logic?._BlinkerOnColor ?? _blinkerOnColor ?? Color.Empty;
            set => _blinkerOnColor = value;
        }

        private KeySequence? _leftBlinkerSequence;
        [JsonProperty("_LeftBlinkerSequence")]
        public KeySequence LeftBlinkerSequence
        {
            get => Logic?._LeftBlinkerSequence ?? _leftBlinkerSequence ?? new KeySequence();
            set => _leftBlinkerSequence = value;
        }

        private KeySequence? _rightBlinkerSequence;
        [JsonProperty("_RightBlinkerSequence")]
        public KeySequence RightBlinkerSequence
        {
            get => Logic?._RightBlinkerSequence ?? _rightBlinkerSequence ?? new KeySequence();
            set => _rightBlinkerSequence = value;
        }

        public Ets2BlinkerLayerHandlerProperties()
        { }
        public Ets2BlinkerLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

        public override void Default() {
            base.Default();

            _leftBlinkerSequence = new KeySequence(new[] { DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4 });
            _rightBlinkerSequence = new KeySequence(new[] { DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12 });
            _blinkerOffColor = Color.Empty;
            _blinkerOnColor = Color.FromArgb(255, 127, 0);
        }
    }

    public class ETS2BlinkerLayerHandler : LayerHandler<Ets2BlinkerLayerHandlerProperties> {
        protected override UserControl CreateControl() {
            return new Control_ETS2BlinkerLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            var blinker_layer = new EffectLayer("ETS2 - Blinker Layer");
            if (gamestate is not GameState_ETS2 stateEts2) return blinker_layer;
            // Left blinker
            var trgColor = stateEts2.Truck.blinkerLeftOn ? Properties.BlinkerOnColor : Properties.BlinkerOffColor;
            blinker_layer.Set(Properties.LeftBlinkerSequence, trgColor);

            // Right blinker
            trgColor = stateEts2.Truck.blinkerRightOn ? Properties.BlinkerOnColor : Properties.BlinkerOffColor;
            blinker_layer.Set(Properties.RightBlinkerSequence, trgColor);
            return blinker_layer;
        }
    }
}
