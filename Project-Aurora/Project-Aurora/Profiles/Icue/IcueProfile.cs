using System.Linq;
using System.Runtime.Serialization;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Icue;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
public class IcueProfile : ApplicationProfile
{
    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        if (Layers.Any(lyr => lyr.Handler is IcueSdkLayerHandler)) return;
        Layers.Add(new Layer("iCUE Lighting", new IcueSdkLayerHandler()));
    }

    public override void Reset()
    {
        base.Reset();
        Layers =
        [
            new Layer("iCUE Lighting", new IcueSdkLayerHandler())
        ];
    }
}