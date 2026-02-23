using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using DrawingColor = System.Drawing.Color;
using System.Windows.Media;
using AuroraRgb.Profiles;
using AuroraRgb.Utils.Json;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace AuroraRgb.Settings.Overrides.Logic;

[JsonObject]
public class OverrideLookupTable : IOverrideLogic {

    /// <summary>The type of variable that the user can set as the output when editing entries.</summary>
    public Type VarType { get; set; }

    /// <summary>The collection of entries that make up this LookupTable.</summary>
    public ObservableCollection<LookupTableEntry> LookupTable { get; set; }

    /// <summary>
    /// Creates a new LookupTable.
    /// </summary>
    /// <param name="varType">The type of variable being edited (e.g. float, System.Drawing.Color, etc.)</param>
    [JsonConstructor]
    public OverrideLookupTable(Type varType) : this(varType, []) { }

    /// <summary>
    /// Creates a new LookupTable.
    /// </summary>
    /// <param name="type">The type of variable being edited (e.g. float, System.Drawing.Color, etc.)</param>
    /// <param name="lookupTable">Optionally a collection of existing LookupTableEntries to add to the new table.</param>
    public OverrideLookupTable(Type type, IEnumerable<LookupTableEntry> lookupTable) {
        VarType = type;
        LookupTable = new ObservableCollection<LookupTableEntry>(lookupTable);
        LookupTable.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LookupTable)));
    }

    /// <summary>
    /// Event that fires when the table changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Adds a new entry to the LookupTable based on the current VarType
    /// </summary>
    public void CreateNewLookup() {
        LookupTable.Add(new LookupTableEntry(Activator.CreateInstance(VarType), new BooleanConstant()));
    }

    /// <summary>
    /// Evalutes this logic and returns the value of the first lookup which has a truthy condition.
    /// Will return `null` if there are no true conditions.
    /// </summary>
    public object? Evaluate(IGameState gameState) {
        foreach (var entry in LookupTable)
            if (entry.Condition.Evaluate(gameState))
                return entry.Value;
        return null;
    }
    
    public bool EvaluateBool(IGameState gameState, out bool overridden) {
        for (var i = 0; i < LookupTable.Count; i++)
        {
            var entry = LookupTable[i];
            if (entry.Condition.Evaluate(gameState))
            {
                overridden = true;
                if (entry.Value is bool b)
                    return b;
                return Convert.ToBoolean(entry.Value);
            }
        }

        overridden = false;
        return false;
    }
    
    public double EvaluateDouble(IGameState gameState, out bool overridden) {
        for (var i = 0; i < LookupTable.Count; i++)
        {
            var entry = LookupTable[i];
            if (entry.Condition.Evaluate(gameState))
            {
                overridden = true;
                if (entry.Value is double d)
                    return d;
                return Convert.ToDouble(entry.Value);
            }
        }

        overridden = false;
        return 0;
    }
    
    public Rectangle EvaluateRectangle(IGameState gameState, out bool overridden) {
        for (var i = 0; i < LookupTable.Count; i++)
        {
            var entry = LookupTable[i];
            if (entry.Condition.Evaluate(gameState))
            {
                overridden = true;
                return entry.Value switch
                {
                    Rectangle r => r,
                    null => default,
                    _ => (Rectangle)Convert.ChangeType(entry.Value, typeof(Rectangle))
                };
            }
        }

        overridden = false;
        return default;
    }
    
    public DrawingColor EvaluateColor(IGameState gameState, out bool overridden) {
        for (var i = 0; i < LookupTable.Count; i++)
        {
            var entry = LookupTable[i];
            if (!entry.Condition.Evaluate(gameState)) continue;
            overridden = true;
            return entry.Value switch
            {
                DrawingColor c => c,
                null => default,
                _ => (DrawingColor)Convert.ChangeType(entry.Value, typeof(Color))
            };
        }

        overridden = false;
        return default;
    }

    /// <summary>
    /// Gets the control allowing the user to edit this LookupTable.
    /// </summary>   
    public Visual GetControl() => _control ??= new Control_OverrideLookupTable(this);
    [JsonIgnore]
    private Control_OverrideLookupTable? _control;

    /// <summary>
    /// Represents a single entry in a LookupTable.
    /// </summary>
    [JsonObject]
    public class LookupTableEntry {
        // The reason for applying a custom converter to this property is so that it is able to handle structs correctly.
        // Structs (atleast in the case of System.Drawing.Color) are simply converted to a string and so when being deserialized
        // into an object field, Newtonsoft JSON converter does not know what kind of struct to parse it as and so it is string
        // type instead of an instance of the struct. The custom converter forcefully wraps a $type parameter around the JSON of
        // the struct so that it can always know the correct class/struct to deserialize the JSON as.
        /// <summary>The value of this entry.</summary>
        [JsonConverter(typeof(TypeAnnotatedObjectConverter))]
        public object? Value { get; set; }

        /// <summary>A boolean condition that should be met for this entry to be valid.</summary>
        public Evaluatable<bool> Condition { get; set; }
            
        public LookupTableEntry(object? value, Evaluatable<bool> condition) {
            Value = value;
            Condition = condition;
        }
    }
}