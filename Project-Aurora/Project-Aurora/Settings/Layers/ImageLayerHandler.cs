using System.Drawing;
using System.IO;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using Newtonsoft.Json;
using Image = System.Drawing.Image;

namespace AuroraRgb.Settings.Layers;

public partial class ImageLayerHandlerProperties : LayerHandlerProperties2Color<ImageLayerHandlerProperties>
{
    private string? _imagePath;

    [JsonProperty("_ImagePath")]
    public string? ImagePath
    {
        get => Logic?._imagePath ?? _imagePath;
        set => _imagePath = value;
    }

    public ImageLayerHandlerProperties()
    { }

    public ImageLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();
        _imagePath = "";
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("SecondaryColor")]
public class ImageLayerHandler() : LayerHandler<ImageLayerHandlerProperties>("ImageLayer")
{
    private Image? _loadedImage;
    private string? _loadedImagePath = "";

    protected override UserControl CreateControl()
    {
        return new Control_ImageLayer(this);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        if (string.IsNullOrWhiteSpace(Properties.ImagePath)) return EffectLayer.EmptyLayer;
        
        if (_loadedImagePath != Properties.ImagePath)
        {
            //Not loaded, load it!
            if (!File.Exists(Properties.ImagePath))
                return EffectLayer.EmptyLayer;

            _loadedImage?.Dispose();
            _loadedImage = new Bitmap(Properties.ImagePath);
            _loadedImagePath = Properties.ImagePath;

            Invalidated = true;

            if (Properties.ImagePath.EndsWith(".gif") && ImageAnimator.CanAnimate(_loadedImage))
                ImageAnimator.Animate(_loadedImage, null);
        }

        if (Properties.ImagePath.EndsWith(".gif") && ImageAnimator.CanAnimate(_loadedImage))
        {
            ImageAnimator.UpdateFrames(_loadedImage);
            Invalidated = true;
        }

        if (Invalidated)
        {
            EffectLayer.Clear();
        }
        
        EffectLayer.DrawTransformed(Properties.Sequence,
            _ => {},
            g =>
            {
                g.DrawImage(_loadedImage);
            },
            new RectangleF(0, 0, _loadedImage.Width, _loadedImage.Height)
        );

        Invalidated = false;
        return EffectLayer;
    }
}