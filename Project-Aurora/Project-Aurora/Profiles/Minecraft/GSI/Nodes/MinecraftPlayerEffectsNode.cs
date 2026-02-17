namespace AuroraRgb.Profiles.Minecraft.GSI.Nodes;

public class MinecraftPlayerEffectsNode {
    public static readonly MinecraftPlayerEffectsNode Default = new();
    
    public bool HasAbsorption { get; set; }
    public bool HasBlindness { get; set; }
    public bool HasFireResistance { get; set; }
    public bool HasInvisibility { get; set; }
    public bool HasNausea { get; set; }
    public bool HasPoison { get; set; }
    public bool HasRegeneration { get; set; }
    public bool HasSlowness { get; set; }
    public bool HasSpeed { get; set; }
    public bool HasWither { get; set; }
}