using Microsoft.CodeAnalysis;

namespace AuroraSourceGenerator.NodeProperties;

public class PropertyAccess(string gsiPath, string accessPath, ITypeSymbol propertyType)
{
    public string GsiPath { get; } = gsiPath;
    public string AccessPath { get; } = accessPath;
    public ITypeSymbol PropertyType { get; } = propertyType;
}