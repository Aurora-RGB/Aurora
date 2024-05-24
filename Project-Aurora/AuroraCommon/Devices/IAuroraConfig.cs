namespace Common.Devices;

public enum JsonSerializerLibrary
{
    SystemText,
    Newtonsoft,
}

public interface IAuroraConfig
{
    public string ConfigPath { get; }
    public JsonSerializerLibrary JsonSerializerLibrary { get; }
}