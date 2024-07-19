using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace AuroraRgb.Settings.Layers;

public partial class LayerHandlerPropertiesLogic : INotifyPropertyChanged
{
    private Color? _primaryColor;

    public Color? _PrimaryColor
    {
        get => _primaryColor;
        set => _primaryColor = value;
    }
    public Color? PrimaryColor
    {
        get => _primaryColor;
        set => _primaryColor = value;
    }

    private KeySequence? _sequence;

    public KeySequence? _Sequence
    {
        get => _sequence;
        set => _sequence = value;
    }
    public KeySequence? Sequence
    {
        get => _sequence;
        set => _sequence = value;
    }

    private bool? _enabled;

    public bool? _Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }
    public bool? Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    private double? _layerOpacity;

    public double? _LayerOpacity
    {
        get => _layerOpacity;
        set => _layerOpacity = value;
    }
    public double? LayerOpacity
    {
        get => _layerOpacity;
        set => _layerOpacity = value;
    }

    private KeySequence _exclusion;
    public KeySequence _Exclusion
    {
        get => _exclusion;
        set => _exclusion = value;
    }
    public KeySequence Exclusion
    {
        get => _exclusion;
        set => _exclusion = value;
    }

    private static readonly Dictionary<string, Action<LayerHandlerPropertiesLogic, object?>> InnerSetters = new()
    {
        ["_PrimaryColor"]	=	(t, v) => {
            if(t._PrimaryColor == (Color?)v) return;
            t._PrimaryColor = (Color?)v;
        },
        ["_Sequence"]	=	(t, v) => {
            if(t._Sequence == v as KeySequence) return;
            t._Sequence = v as KeySequence;
        },
        ["_Enabled"]	=	(t, v) => {
            if(t._Enabled == (bool?)v) return;
            t._Enabled = (bool?)v;
        },
        ["_LayerOpacity"]	=	(t, v) => {
            if(t._LayerOpacity == (double?)v) return;
            t._LayerOpacity = (double?)v;
        },
        ["_Exclusion"]	=	(t, v) => {
            if(t._Exclusion == v as KeySequence) return;
            t._Exclusion = v as KeySequence;
        },
        
        ["PrimaryColor"]	=	(t, v) => {
            if(t._PrimaryColor == (Color?)v) return;
            t._PrimaryColor = (Color?)v;
        },
        ["Sequence"]	=	(t, v) => {
            if(t._Sequence == v as KeySequence) return;
            t._Sequence = v as KeySequence;
        },
        ["Enabled"]	=	(t, v) => {
            if(t._Enabled == (bool?)v) return;
            t._Enabled = (bool?)v;
        },
        ["LayerOpacity"]	=	(t, v) => {
            if(t._LayerOpacity == (double?)v) return;
            t._LayerOpacity = (double?)v;
        },
        ["Exclusion"]	=	(t, v) => {
            if(t._Exclusion == v as KeySequence) return;
            t._Exclusion = v as KeySequence;
        }
    };

    public virtual IReadOnlyDictionary<string, Action<LayerHandlerPropertiesLogic, object?>> SetterMap => InnerSetters;
}

public class LayerHandlerPropertiesLogic<T> : LayerHandlerPropertiesLogic;