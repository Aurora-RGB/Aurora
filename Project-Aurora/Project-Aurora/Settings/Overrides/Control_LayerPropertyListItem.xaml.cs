using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Settings.Overrides.Logic;

namespace AuroraRgb.Settings.Overrides;

public partial class Control_LayerPropertyListItem
{
    public Control_LayerPropertyListItem()
    {
        InitializeComponent();
        
        // init design time data
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new LayerPropertyViewModel("PropertyName", "Display Name", typeof(int));
        }
    }
}


public class OverrideTypeToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((Type)value! == typeof(OverrideDynamicValue))
            return Brushes.MediumPurple;
        if ((Type)value! == typeof(OverrideLookupTable))
            return Brushes.Teal;
        return Brushes.DimGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}