namespace AuroraRgb.Settings.Layers;

public interface IValueOverridable {

    /// <summary>
    /// Sets the overriden value of the speicifed property to the given value.
    /// </summary>
    void SetOverride(string propertyName, object? value);
}