using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator.NodeProperties;

public class PropertyLookupInfo
{
    public string Name { get; }
    public string GsiPath { get; }
    public string? AccessPath { get; }
    public bool IsFolder { get; }
    public ITypeSymbol? PropertyType { get; }
    public string Description { get; } = string.Empty;

    public PropertyLookupInfo(string name, string gsiPath, string accessPath, ITypeSymbol? type)
    {
        Name = name;
        GsiPath = gsiPath;
        AccessPath = accessPath;
        IsFolder = false;
        PropertyType = type;
    }

    public PropertyLookupInfo(string name, string gsiPath, string? description)
    {
        Name = name;
        GsiPath = gsiPath;
        IsFolder = true;
        Description = description ?? string.Empty;
    }
}