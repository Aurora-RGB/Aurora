using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;

namespace AuroraRgb.Settings.Layers;

public interface ILayerHandler: IDisposable
{
    Task<UserControl> Control { get; }

    object Properties { get; set; }

    bool EnableSmoothing { get; set; }

    bool EnableExclusionMask { get; }
    bool? _EnableExclusionMask { get; set; }

    KeySequence ExclusionMask { get; }
    KeySequence _ExclusionMask { get; set; }

    float Opacity { get; }
    float? _Opacity { get; set; }

    EffectLayer Render(IGameState gameState);

    EffectLayer PostRenderFX(EffectLayer layerRender);

    void SetApplication(Application profile);
    void SetGameState(IGameState gamestate);
    void Dispose();
}