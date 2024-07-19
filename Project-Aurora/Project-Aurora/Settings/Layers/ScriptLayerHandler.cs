using System;
using System.ComponentModel;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public partial class ScriptLayerHandlerProperties : LayerHandlerProperties<ScriptLayerHandlerProperties>
{
    private string? _script;
    [JsonProperty("_Script")]
    public string Script
    {
        get => Logic?._script ?? _script ?? string.Empty;
        set
        {
            if (_script == value)
            {
                return;
            }
            _script = value;
            OnPropertiesChanged(this, new PropertyChangedEventArgs(nameof(Script)));
        }
    }

    private VariableRegistry? _scriptProperties;

    [JsonProperty("_ScriptProperties")]
    public VariableRegistry ScriptProperties
    {
        get => Logic?._scriptProperties ?? _scriptProperties ?? throw new NullReferenceException("ScriptLayerHandlerProperties._scriptProperties is null");
        set => _scriptProperties = value;
    }

    public ScriptLayerHandlerProperties() { }

    public ScriptLayerHandlerProperties(bool empty = false) : base(empty) { }

    public override void Default()
    {
        base.Default();
        _scriptProperties = new VariableRegistry();
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
public class ScriptLayerHandler : LayerHandler<ScriptLayerHandlerProperties>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonIgnore]
    public Exception ScriptException { get; private set; }

    public override EffectLayer Render(IGameState gamestate)
    {

        if (!IsScriptValid) return EffectLayer.EmptyLayer;
        try
        {
            var script = Application.EffectScripts[Properties.Script];
            var scriptLayers = script.UpdateLights(Properties.ScriptProperties, gamestate);
            EffectLayer.Clear();
            switch (scriptLayers)
            {
                case EffectLayer layers:
                    EffectLayer.Add(layers);
                    break;
                case EffectLayer[] effectLayers:
                {
                    for (var i = 1; i < effectLayers.Length; i++)
                        EffectLayer.Add(effectLayers[i]);
                    break;
                }
            }
            ScriptException = null;
        }
        catch(Exception exc)
        {
            Global.logger.Error(exc, "Effect script with key {PropertiesScript} encountered an error", Properties.Script);
            ScriptException = exc;
        }

        return EffectLayer;
    }

    public VariableRegistry? GetScriptPropertyRegistry()
    {
        if (IsScriptValid)
        {
            return Application.EffectScripts[Properties.Script].Properties;
        }

        return null;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        if (args.PropertyName is nameof(Properties.Script))
        {
            OnScriptChanged();
        }
    }

    private void OnScriptChanged()
    {
        if (Properties.Script == null || Application == null || !Application.EffectScripts.ContainsKey(Properties.Script))
        {
            return;
        }
        Properties.ScriptProperties = (VariableRegistry)Application.EffectScripts[Properties.Script].Properties.Clone();
    }

    [JsonIgnore]
    public bool  IsScriptValid => Application?.EffectScripts?.ContainsKey(Properties.Script) ?? false;

    public override void SetApplication(Application profile)
    {
        Application = profile;

        if (Application.EffectScripts.ContainsKey(Properties.Script))
        {
            var scriptRegistry = (VariableRegistry)Application.EffectScripts[Properties.Script].Properties.Clone();
            Properties.ScriptProperties.Combine(scriptRegistry, true);
        }

        base.SetApplication(profile);
    }

    protected override UserControl CreateControl()
    {
        return new Control_ScriptLayer(this);
    }
}