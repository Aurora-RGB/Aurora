using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator.NodeProperties;

public class PropertyLookupInfo
{
    public string Name { get; }
    public string GsiPath { get; }
    public bool IsFolder { get; }
    public ITypeSymbol? PropertyType { get; }

    public PropertyLookupInfo(string name, string gsiPath, ITypeSymbol? type)
    {
        Name = name;
        GsiPath = gsiPath;
        IsFolder = false;
        PropertyType = type;
    }

    public PropertyLookupInfo(string name, string gsiPath)
    {
        Name = name;
        GsiPath = gsiPath;
        IsFolder = true;
    }
}