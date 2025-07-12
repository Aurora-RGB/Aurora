using System;
using System.Collections.Generic;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;
using CoordinateSharp;

namespace AuroraRgb.Nodes;

[GameStateDescription(Description)]
public class CelestialData : Node
{
    private const string Description = """
                                       Provides the solar noon percentage, which indicates how close the sun is to the zenith.
                                       """;

    private int _currentHour = -1;
    private Dictionary<int, double> _currentHourValues = new();
    
    
    private static readonly EagerLoad El = new(EagerLoadType.Celestial)
    {
        Extensions = new EagerLoad_Extensions(EagerLoad_ExtensionsType.Solar_Cycle)
    };
    private Coordinate Coordinate { get; set; } = new(0, 0, El);
    private bool _invalidated = true;

    public CelestialData()
    {
        Global.SensitiveData.PropertyChanged += (_, propertyChangedEvent) =>
        {
            if (propertyChangedEvent.PropertyName is not (nameof(SensitiveData.Lat) or nameof(SensitiveData.Lon))) return;

            _currentHour = -1;
            _invalidated = true;
        };
    }

    public double SolarNoonPercentage
    {
        get
        {
            if (_invalidated)
            {
                Coordinate = new Coordinate(Global.SensitiveData.Lat, Global.SensitiveData.Lon, DateTime.UtcNow, El);
                _invalidated = false;
            }
            
            var dateTime = DateTime.UtcNow;

            if (dateTime.Hour != _currentHour)
            {
                _currentHour = dateTime.Hour;
                GenerateHourData(dateTime);
            }

            return _currentHourValues[dateTime.Minute];
        }
    }

    private void GenerateHourData(DateTime date)
    {
        _currentHourValues = new Dictionary<int, double>();

        date = date.AddMinutes(-date.Minute);
        for (var i = 0; i < 60; i++)
        {
            Coordinate.GeoDate = date;
            var noonPercentage = BrightnessFunction(Coordinate.CelestialInfo.SunAltitude);
            _currentHourValues[i] = Math.Clamp(noonPercentage, 0d, 100d);
            date = date.AddMinutes(1);
        }
    }

    private static double BrightnessFunction(double x)
    {
        const double noonAngle = 90;        //x2
        const double darknessAngle = -18;   //x1
        //linear plotting   (y2 - y1)(x2 - x1)
        const double m = (100 - 0) / (noonAngle - darknessAngle);
        
        // y = m(x - x1) + y1
        return m * (x - darknessAngle) + 0;
    }
}