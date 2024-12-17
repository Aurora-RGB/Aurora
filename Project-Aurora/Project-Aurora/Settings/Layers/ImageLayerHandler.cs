using System.ComponentModel;
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

public partial class ImageLayerHandlerProperties : LayerHandlerProperties2Color
{
    private string _imagePath = string.Empty;

    [JsonProperty("_ImagePath")]
    public string ImagePath
    {
        get => Logic?._imagePath ?? _imagePath;
        set
        {
            _imagePath = value;
            OnPropertiesChanged(this, new PropertyChangedEventArgs(nameof(ImagePath)));
        }
    }

    public override void Default()
    {
        base.Default();
        ImagePath = "";
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("SecondaryColor")]
public class ImageLayerHandler() : LayerHandler<ImageLayerHandlerProperties, BitmapEffectLayer>("ImageLayer")
{
    private Image _loadedImage = new Bitmap(8, 8);
    private string? _loadedImagePath = "";
    private bool _secondInvalidation = true;

    protected override UserControl CreateControl()
    {
        return new Control_ImageLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (string.IsNullOrWhiteSpace(Properties.ImagePath)) return EmptyLayer.Instance;

        if (_loadedImagePath != Properties.ImagePath)
        {
            //Not loaded, load it!
            if (!File.Exists(Properties.ImagePath))
                return EmptyLayer.Instance;

            _loadedImage.Dispose();
            _loadedImage = new Bitmap(Properties.ImagePath);
            _loadedImagePath = Properties.ImagePath;

            Invalidated = true;

            if (Properties.ImagePath.EndsWith(".gif") && ImageAnimator.CanAnimate(_loadedImage))
                ImageAnimator.Animate(_loadedImage, (_, _) => { });
        }

        if (Properties.ImagePath.EndsWith(".gif") && ImageAnimator.CanAnimate(_loadedImage))
        {
            ImageAnimator.UpdateFrames(_loadedImage);
            Invalidated = true;
        }

        if (!Invalidated && !_secondInvalidation)
        {
            return EffectLayer;
        }

        EffectLayer.Clear();
        EffectLayer.DrawTransformed(Properties.Sequence,
            g => g.DrawImage(_loadedImage),
            new RectangleF(0, 0, _loadedImage.Width, _loadedImage.Height)
        );

        if (!Invalidated)
        {
            Invalidated = false;
        }
        else
        {
            // don't know why but image needs to be drawn two times when canvas changes or first initialized
            _secondInvalidation = false;
        }
        return EffectLayer;
    }
}