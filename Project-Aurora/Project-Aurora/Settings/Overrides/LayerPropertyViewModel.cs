using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides;

public partial class LayerPropertyViewModel : INotifyPropertyChanged
{
    public string PropertyName { get; }
    public string DisplayName { get; }
    public Type PropertyType { get; }
    public Type? OverrideType { get; set; }

    public LayerPropertyViewModel(PropertyInfo layerPropertyPropertyInfo, Layer layer)
    {
        // handle change of porperty's override type from layer 
        layer.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(Layer.OverrideLogic)) return;
            if (layer.OverrideLogic.TryGetValue(layerPropertyPropertyInfo.Name, out var value))
            {
                if (OverrideType == value.GetType()) return;
                OverrideType = value.GetType();
            }
            else
            {
                OverrideType = null;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverrideType)));
        };
        
        PropertyName = layerPropertyPropertyInfo.Name;
        DisplayName =
            layerPropertyPropertyInfo.GetCustomAttributes<LogicOverridableAttribute>(true).FirstOrDefault()
            ?.Name // Get the name specified in the attribute (so it is prettier for the user),
            ?? layerPropertyPropertyInfo.Name.TrimStart('_').CamelCaseToSpaceCase(); //  but if one wasn't provided, pretty-print the code name
        PropertyType = Nullable.GetUnderlyingType(layerPropertyPropertyInfo.PropertyType) ??
                       layerPropertyPropertyInfo.PropertyType; // If the property is a nullable type (e.g. bool?), will instead return the non-nullable type (bool)
    }

    public LayerPropertyViewModel(string propertyName, string displayName, Type propertyType)
    {
        PropertyName = propertyName;
        DisplayName = displayName;
        PropertyType = propertyType;
    }
}