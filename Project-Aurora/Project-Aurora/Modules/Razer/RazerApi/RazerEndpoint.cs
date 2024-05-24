namespace AuroraRgb.Modules.Razer.RazerApi;

public class RazerEndpoint(string name, string hash)
{
    public string Name { get; } = name;
    public string Hash { get; } = hash;
}