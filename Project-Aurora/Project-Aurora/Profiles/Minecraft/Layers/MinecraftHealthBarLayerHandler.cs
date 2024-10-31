using System;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Minecraft.GSI;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Minecraft.Layers {
    [Obsolete("This layer is obselete and has been replaced by the Overrides system.")]
    public partial class MinecraftHealthBarLayerHandlerProperties : LayerHandlerProperties {

        [JsonIgnore]
        public Color NormalHealthColor => _NormalHealthColor ?? Color.Empty;
        public Color? _NormalHealthColor { get; set; }

        [JsonIgnore]
        public bool EnableAbsorptionHealthColor => _EnableAbsorptionHealthColor ?? false;
        public bool? _EnableAbsorptionHealthColor { get; set; }
        [JsonIgnore]
        public Color AbsorptionHealthColor => _AbsorptionHealthColor ?? Color.Empty;
        public Color? _AbsorptionHealthColor { get; set; }

        [JsonIgnore]
        public bool EnableRegenerationHealthColor => _EnableRegenerationHealthColor ?? false;
        public bool? _EnableRegenerationHealthColor { get; set; }
        [JsonIgnore]
        public Color RegenerationHealthColor => _RegenerationHealthColor ?? Color.Empty;
        public Color? _RegenerationHealthColor { get; set; }

        [JsonIgnore]
        public bool EnablePoisonHealthColor => _EnablePoisonHealthColor ?? false;
        public bool? _EnablePoisonHealthColor { get; set; }
        [JsonIgnore]
        public Color PoisonHealthColor => _PoisonHealthColor ?? Color.Empty;
        public Color? _PoisonHealthColor { get; set; }

        [JsonIgnore]
        public bool EnableWitherHealthColor => _EnableWitherHealthColor ?? false;
        public bool? _EnableWitherHealthColor { get; set; }
        [JsonIgnore]
        public Color WitherHealthColor => _WitherHealthColor ?? Color.Empty;
        public Color? _WitherHealthColor { get; set; }

        [JsonIgnore]
        public Color BackgroundColor => _BackgroundColor ?? Color.Empty;
        public Color? _BackgroundColor { get; set; }

        [JsonIgnore]
        public bool GradualProgress => _GradualProgress ?? false;
        public bool? _GradualProgress { get; set; }

        public override void Default() {
            base.Default();
            _NormalHealthColor = Color.Red;
            _AbsorptionHealthColor = Color.FromArgb(255, 210, 0);
            _RegenerationHealthColor = Color.FromArgb(240, 75, 100);
            _PoisonHealthColor = Color.FromArgb(145, 160, 30);
            _WitherHealthColor = Color.FromArgb(70, 5, 5);
            _BackgroundColor = Color.Transparent;
            _EnableAbsorptionHealthColor = _EnableRegenerationHealthColor = _EnablePoisonHealthColor = _EnableWitherHealthColor = true;
            _GradualProgress = false;
        }
    }

    public class MinecraftHealthBarLayerHandler() : LayerHandler<MinecraftHealthBarLayerHandlerProperties, BitmapEffectLayer>("Minecraft Health Bar Layer")
    {
        protected override UserControl CreateControl() {
            return new Control_MinecraftHealthBarLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            // Ensure the gamestate is for Minecraft, and store a casted reference to it
            if (gamestate is not GameStateMinecraft minecraftState) return EmptyLayer.Instance;

            // Choose the main healthbar's color depending on whether the player is withered/poisoned/regen/normal.
            var barColor = Properties.NormalHealthColor; // Default normal color
            if (Properties.EnableWitherHealthColor && minecraftState.Player.PlayerEffects.HasWither) // Wither takes priority over others
                barColor = Properties.WitherHealthColor;
            else if (Properties.EnablePoisonHealthColor && minecraftState.Player.PlayerEffects.HasPoison) // Poison 2nd priority
                barColor = Properties.PoisonHealthColor;
            else if (Properties.EnableRegenerationHealthColor && minecraftState.Player.PlayerEffects.HasRegeneration) // Regen 3rd priority
                barColor = Properties.RegenerationHealthColor;

            // Render the main healthbar, with the color decided above.
            EffectLayer.PercentEffect(barColor, Properties.BackgroundColor, Properties.Sequence, minecraftState.Player.Health, minecraftState.Player.HealthMax);

            // If absorption is enabled, overlay the absorption display on the top of the original healthbar
            if (Properties.EnableAbsorptionHealthColor)
                EffectLayer.PercentEffect(Properties.AbsorptionHealthColor, Properties.BackgroundColor, Properties.Sequence, minecraftState.Player.Absorption, minecraftState.Player.AbsorptionMax, Properties.GradualProgress ? PercentEffectType.Progressive_Gradual : PercentEffectType.Progressive);

            return EffectLayer;
        }
    }
}
