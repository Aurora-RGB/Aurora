using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Chroma;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
public class RazerChromaProfile : ApplicationProfile
{
    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        // #292: the standalone event-driven layer renders on top (the Razer layer below is the Synapse path).
        if (Layers.All(lyr => lyr.Handler.GetType() != typeof(ChromaEventLayerHandler)))
            Layers.Insert(0, new Layer("Chroma Event (Wyvrn)", new ChromaEventLayerHandler()));

        if (Layers.Any(lyr => lyr.Handler.GetType().Equals(typeof(RazerLayerHandler)))) return;
        Layers.Add(new Layer("Chroma Lighting", new RazerLayerHandler()));
        var solidFillLayerHandler = new SolidFillLayerHandler();
        solidFillLayerHandler.Properties._PrimaryColor = CommonColorUtils.FastColor(255, 255, 255, 24);
        Layers.Add(new("Background", solidFillLayerHandler));
    }

    public override void Reset()
    {
        base.Reset();
        var solidFillLayerHandler = new SolidFillLayerHandler();
        solidFillLayerHandler.Properties._PrimaryColor = CommonColorUtils.FastColor(255, 255, 255, 24);
        Layers = new ObservableCollection<Layer>
        {
            new("Chroma Event (Wyvrn)", new ChromaEventLayerHandler()),
            new("Chroma Lighting", new RazerLayerHandler()),
            new("Background", solidFillLayerHandler),
        };
    }
}