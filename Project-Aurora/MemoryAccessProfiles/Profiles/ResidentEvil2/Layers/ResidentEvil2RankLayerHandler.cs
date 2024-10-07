using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;
using MemoryAccessProfiles.Profiles.ResidentEvil2.GSI;
using MemoryAccessProfiles.Profiles.ResidentEvil2.GSI.Nodes;

namespace MemoryAccessProfiles.Profiles.ResidentEvil2.Layers;

public partial class ResidentEvil2RankLayerHandlerProperties : LayerHandlerProperties2Color
{
    public override void Default()
    {
        base.Default();
        _Sequence = new KeySequence(new[] {
            DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE,
            DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE
        });
    }

}

public class ResidentEvil2RankLayerHandler : LayerHandler<ResidentEvil2RankLayerHandlerProperties>
{
    protected override UserControl CreateControl()
    {
        return new Control_ResidentEvil2RankLayer( this );
    }

    public override EffectLayer Render(IGameState state)
    {
        EffectLayer keys_layer = new EffectLayer( "Resident Evil 2 - Rank" );

        if (state is GameState_ResidentEvil2)
        {
            GameState_ResidentEvil2 re2state = state as GameState_ResidentEvil2;

            if (re2state.Player.Status != Player_ResidentEvil2.PlayerStatus.OffGame && re2state.Player.Rank != 0)
            {
                keys_layer.Set(Properties.Sequence.Keys[re2state.Player.Rank - 1], Color.White);
            }
        }
        return keys_layer;
    }
}