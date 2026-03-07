using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Discord.GSI;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Discord.Layers;

public partial class DiscordVoiceActivityLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _defaultColor;
    public Color DefaultColor
    {
        get => Logic?._DefaultColor ?? _defaultColor ?? Color.Empty;
        set => _defaultColor = value;
    }

    private Color? _speakingColor;
    public Color SpeakingColor
    {
        get => Logic?.SpeakingColor ?? _speakingColor ?? Color.Empty;
        set => _speakingColor = value;
    }

    private Color? _mutedColor;
    public Color MutedColor
    {
        get => Logic?.MutedColor ?? _mutedColor ?? Color.Empty;
        set => _mutedColor = value;
    }

    public override void Default()
    {
        base.Default();

        _defaultColor = Color.FromArgb(0, 124, 255);
        _speakingColor = Color.FromArgb(33, 255, 40);
        _mutedColor = Color.Red;
    }
}

public class DiscordVoiceActivityLayerHandler() : LayerHandler<DiscordVoiceActivityLayerHandlerProperties>("Discord Layer Activity")
{
    private readonly Color _transparent = Color.Transparent;
    private int _lastParticipantCount;

    protected override UserControl CreateControl()
    {
        return new ControlDiscordVoiceActivityLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateDiscord discordState) return EmptyLayer.Instance;

        if (Invalidated || discordState.Participants.Count != _lastParticipantCount)
        {
            EffectLayer.Clear();
            Invalidated = false;
        }

        _lastParticipantCount = discordState.Participants.Count;
        var participantIds = discordState.Participants.Keys;
        var keySequence = Properties.Sequence.Keys;

        var index = 0;
        foreach (var key in keySequence)
        {
            var participantId = participantIds.ElementAtOrDefault(index++);
            if (participantId == null)
            {
                EffectLayer.Set(key, in _transparent);
                continue;
            }

            var participant = discordState.Participants[participantId];

            if (participant.IsMuted || participant.IsSelfMuted)
                EffectLayer.Set(key, Properties.MutedColor);
            else if (participant.IsSpeaking)
                EffectLayer.Set(key, Properties.SpeakingColor);
            else
                EffectLayer.Set(key, Properties.DefaultColor);
        }

        return EffectLayer;
    }
}