using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Inputs;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Devices;
using Newtonsoft.Json;
using UserControl = System.Windows.Controls.UserControl;

namespace AuroraRgb.Settings.Layers;

public enum ShortcutAssistantPresentationType
{
    [Description("Show All Keys")]
    Default = 0,

    [Description("Progressive Suggestion")]
    ProgressiveSuggestion = 1
}

public partial class ShortcutAssistantLayerHandlerProperties : LayerHandlerProperties
{
    [JsonIgnore]
    private bool? _dimBackground;

    [JsonProperty("_DimBackground")]
    [LogicOverridable("Dim Background")]
    public bool DimBackground
    {
        get => (Logic?._dimBackground ?? _dimBackground) ?? false;
        set => _dimBackground = value;
    }

    [JsonIgnore]
    private bool? _mergeModifierKey;

    [JsonProperty("_MergeModifierKey")]
    public bool MergeModifierKey {
        get => (Logic?._mergeModifierKey ?? _mergeModifierKey) ?? false;
        set { _mergeModifierKey = value; _shortcutKeysInvalidated = true; }
    }

    [JsonIgnore]
    private bool? _leafShortcutAlwaysOn;

    [JsonProperty("_LeafShortcutAlwaysOn")]
    public bool? LeafShortcutAlwaysOn
    {
        get => (Logic?._leafShortcutAlwaysOn ?? _leafShortcutAlwaysOn) ?? false;
        set { _leafShortcutAlwaysOn = value; _shortcutKeysInvalidated = true; }
    }

    [JsonIgnore]
    private Color? _dimColor;

    [JsonProperty("_DimColor")]
    [LogicOverridable("Dim Color")]
    public Color DimColor
    {
        get => (Logic?._dimColor ?? _dimColor) ?? Color.Empty;
        set => _dimColor = value;
    }

    [JsonIgnore]
    private Keybind[] _shortcutKeys = Array.Empty<Keybind>();

    [JsonProperty("_ShortcutKeys")]
    public Keybind[] ShortcutKeys
    {
        get => _shortcutKeys;
        set { _shortcutKeys = value; _shortcutKeysInvalidated = true; }
    }

    [JsonIgnore]
    private bool _shortcutKeysInvalidated = true;

    [JsonIgnore]
    private Tree<Keys> _shortcutKeysTree = new(Keys.None);

    [JsonIgnore]
    public Tree<Keys> ShortcutKeysTree
    {
        get
        {
            if (!_shortcutKeysInvalidated) return _shortcutKeysTree;
            _shortcutKeysTree = new Tree<Keys>(Keys.None);

            foreach (var keybind in ShortcutKeys)
            {
                var keys = keybind.ToArray();

                if (MergeModifierKey)
                {
                    for (var i = 0; i < keys.Length; i++)
                        keys[i] = KeyUtils.GetStandardKey(keys[i]);
                }

                _shortcutKeysTree.AddBranch(keys);
            }

            _shortcutKeysInvalidated = false;
            return _shortcutKeysTree;
        }
    }

    [JsonIgnore]
    private ShortcutAssistantPresentationType? _presentationType;

    [JsonProperty("_PresentationType")]
    public ShortcutAssistantPresentationType PresentationType
    {
        get => Logic?._presentationType ?? _presentationType ?? ShortcutAssistantPresentationType.ProgressiveSuggestion;
        set => _presentationType = value;
    }

    public override void Default()
    {
        base.Default();
        _dimBackground = true;
        DimColor = Color.FromArgb(169, 0, 0, 0);
        PresentationType = ShortcutAssistantPresentationType.Default;
        _mergeModifierKey = false;
    }
}

public class ShortcutAssistantLayerHandler() : LayerHandler<ShortcutAssistantLayerHandlerProperties>("Shortcut Assistant")
{
    private bool _init;
    private IReadOnlyCollection<Keys>? _heldKeys;

    protected override UserControl CreateControl()
    {
        return new ControlShortcutAssistantLayer(this);
    }

    /// <summary>
    /// Check if layer effect is active based on what keys are pressed. 
    /// Layer is considered active if the first pressed key is a child of the root of Properties.ShortcutKeysTree
    /// </summary>
    /// <returns>true if layer is active</returns>
    private bool IsLayerActive([MaybeNullWhen(false)] out IReadOnlyCollection<Keys> heldKeys)
    {
        if (!_init)
        {
            _init = true;
            InputsModule.InputEvents.Result.KeyDown += OnKeyDown;
            InputsModule.InputEvents.Result.KeyUp += OnKeyUp;

            heldKeys = _heldKeys;
            return _heldKeys != null;
        }

        heldKeys = _heldKeys;
        return _heldKeys != null;
    }

    private void OnKeyDown(object? sender, KeyboardKeyEventArgs e)
    {
        var shouldActivate = ShouldActivate(e.PressedKeys);
        if (shouldActivate)
        {
            _heldKeys = e.PressedKeys;
        }
    }

    private void OnKeyUp(object? sender, KeyboardKeyEventArgs e)
    {
        var shouldActivate = ShouldActivate(e.PressedKeys);
        if (!shouldActivate)
        {
            _heldKeys = null;
        }
    }

    private bool ShouldActivate(IEnumerable<Keys> heldKeys)
    {
        var keyToCheck = heldKeys.FirstOrDefault();

        var key = Properties.MergeModifierKey ? KeyUtils.GetStandardKey(keyToCheck) : keyToCheck;
        var shouldActivate = Properties.ShortcutKeysTree.ContainsItem(key) != null;
        return shouldActivate;
    }

    private Keys[] MatchHeldKeysToShortcutTree(IEnumerable<Keys> heldKeys, Tree<Keys> shortcuts)
    {
        var currentShortcutNode = shortcuts;
        var heldKeysToHighlight = Array.Empty<Keys>();

        foreach (var key in heldKeys)
        {
            var child = currentShortcutNode.ContainsItem(key);
            if (child != null)
            {
                currentShortcutNode = child;
                heldKeysToHighlight = heldKeysToHighlight.Append(key).ToArray();
            }
            else
            {
                break;
            }
        }

        return heldKeysToHighlight;
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        if (!IsLayerActive(out var heldKeys))
        {
            return EmptyLayer.Instance;
        }

        // The layer is active. At this point we have at least 1 key to highlight
        var heldKeysToHighlight = MatchHeldKeysToShortcutTree(heldKeys, Properties.ShortcutKeysTree); // This is also the path in shortcut tree

        var currentShortcutNode = Properties.ShortcutKeysTree.GetNodeByPath(heldKeysToHighlight);
        if (currentShortcutNode == null)
        {
            return EmptyLayer.Instance;
        }
        if (Properties.LeafShortcutAlwaysOn.GetValueOrDefault(false) && currentShortcutNode.IsLeaf)
        {
            // Go down one level
            currentShortcutNode = Properties.ShortcutKeysTree.GetNodeByPath(heldKeysToHighlight.Take(heldKeysToHighlight.Length - 1).ToArray());
            if (currentShortcutNode == null)
            {
                return EmptyLayer.Instance;
            }
        }

        // Convert to DeviceKeys
        var selectedKeys = BuildSelectedKeys(currentShortcutNode, heldKeysToHighlight);
        var backgroundKeys = KeyUtils.GetDeviceAllKeys().Except(selectedKeys).ToArray();

        // Display keys
        ApplyKeyColors(EffectLayer, selectedKeys, backgroundKeys);

        return EffectLayer;
    }

    private DeviceKeys[] BuildSelectedKeys(Tree<Keys> currentShortcutNode, Keys[] previousShortcutKeys)
    {
        Keys[] nextPossibleShortcutKeys;
        switch (Properties.PresentationType)
        {
            default:
            case ShortcutAssistantPresentationType.Default:
                nextPossibleShortcutKeys = currentShortcutNode.GetAllChildren();
                break;
            case ShortcutAssistantPresentationType.ProgressiveSuggestion:
                nextPossibleShortcutKeys = currentShortcutNode.GetChildren();
                break;
        }

        return KeyUtils.GetDeviceKeys(nextPossibleShortcutKeys, true, !Console.NumberLock)
            .Concat(KeyUtils.GetDeviceKeys(previousShortcutKeys, true)).ToArray();
    }

    private void ApplyKeyColors(EffectLayer layer, DeviceKeys[] selectedKeys, DeviceKeys[] backgroundKeys)
    {
        if (Properties.DimBackground)
        {
            layer.Set(backgroundKeys, Properties.DimColor);
        }

        layer.Set(selectedKeys, Properties.PrimaryColor);
    }

    public override void Dispose()
    {
        InputsModule.InputEvents.Result.KeyDown -= OnKeyDown;
        InputsModule.InputEvents.Result.KeyUp -= OnKeyUp;
        base.Dispose();
    }
}