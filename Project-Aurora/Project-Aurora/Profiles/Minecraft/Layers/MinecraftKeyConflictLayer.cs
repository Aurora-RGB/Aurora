using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Minecraft.GSI;
using AuroraRgb.Settings.Layers;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Minecraft.Layers;

public partial class MinecraftKeyConflictLayerProperties : LayerHandlerProperties2Color {

    // PrimaryColor -> No conflict
    // SecondaryColor -> Hard conflict
    // TertiaryColor -> Soft conflict

    [JsonProperty("_TertiaryColor")]
    public Color TertiaryColor
    {
        get => Logic?._tertiaryColor ?? field;
        set;
    }

    public override void Default() {
        base.Default();
        PrimaryColor = Color.FromArgb(0, 255, 0);
        SecondaryColor = Color.FromArgb(255, 0, 0);
        TertiaryColor = Color.FromArgb(255, 127, 0);
    }
}


public sealed class MinecraftKeyConflictLayerHandler() : LayerHandler<MinecraftKeyConflictLayerProperties>("Minecraft Key Conflict Layer")
{
    private readonly Color _backgroundColor = Color.Black;

    protected override UserControl CreateControl() {
        return new Control_MinecraftKeyConflictLayer(this);
    }

    public override EffectLayer Render(IGameState gameState) {
        if (gameState is not GameStateMinecraft minecraftState || !minecraftState.Game.ControlsGuiOpen)
        {
            return EmptyLayer.Instance;
        }

        EffectLayer.Fill(in _backgroundColor); // Hide any other layers behind this one
        // Set all keys in use by any binding to be the no-conflict colour
        foreach (var kb in minecraftState.Game.KeyBindings)
            EffectLayer.Set(kb.AffectedKeys, Properties.PrimaryColor);

        // Override the keys for all conflicting keys
        foreach (var kvp in CalculateConflicts(minecraftState))
            EffectLayer.Set(kvp.Key, kvp.Value ? Properties.TertiaryColor : Properties.SecondaryColor);
        return EffectLayer;
    }

    /// <summary>
    /// Method that calculates the key conflicts based on the GameState's Game.KeyBindings property.
    /// Returns an enumerable of all DeviceKeys with a conflict, and whether they are only modifier conflicts (warning).
    /// Forge shows modifier conflicts in red and soft in orange on the in-game keys menu.
    /// </summary>
    private static Dictionary<DeviceKeys, bool> CalculateConflicts(GameStateMinecraft state) {
        var keys = new Dictionary<DeviceKeys, bool>();
        foreach (var bind in state.Game.KeyBindings) { // For every key binding

            // This code is based on the code from Minecraft in "GuiKeyBindingList.java" in the "drawEntry" method.
            // It may not be the most efficient way of computing conflicts but I'm struggling to entirely follow
            // the logic in the Minecraft code, so I've decided to replicate it to prevent conflicts
            var hasConflict = false;
            var isOnlyModifierConflict = true;

            foreach (var otherBind in state.Game.KeyBindings)
            {
                // Check against every other key binding
                if (bind == otherBind || !otherBind.ConflictsWith(bind)) continue;
                hasConflict = true;
                isOnlyModifierConflict &= otherBind.ModifierConflictsWith(bind);
            }
            // End replicated section

            if (!hasConflict) continue;
            foreach (var affectedKey in bind.AffectedKeys) // For each key that is affected by this keybind
                keys[affectedKey] = keys.TryGetValue(affectedKey, out var key) // Check if this key is already flagged as a conflict
                    ? key && isOnlyModifierConflict // If so, ensure it shows full conflicts over modifier conflicts 
                    : isOnlyModifierConflict; // Else if not already flagged, simply set it.
        }
        return keys;
    }
}