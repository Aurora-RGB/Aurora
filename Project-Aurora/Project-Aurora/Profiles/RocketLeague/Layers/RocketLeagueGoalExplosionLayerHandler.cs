using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Profiles.RocketLeague.GSI;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.RocketLeague.Layers;

public partial class RocketLeagueGoalExplosionProperties : LayerHandlerProperties
{
    private bool? _showFriendlyGoalExplosion;

    [JsonProperty("_ShowFriendlyGoalExplosion")]
    public bool ShowFriendlyGoalExplosion
    {
        get => Logic?._showFriendlyGoalExplosion ?? _showFriendlyGoalExplosion ?? true;
        set => _showFriendlyGoalExplosion = value;
    }

    private bool? _showEnemyGoalExplosion;

    [JsonProperty("_ShowEnemyGoalExplosion")]
    public bool ShowEnemyGoalExplosion
    {
        get => Logic?._showEnemyGoalExplosion ?? _showEnemyGoalExplosion ?? true;
        set => _showEnemyGoalExplosion = value;
    }

    private bool? _background;

    [JsonProperty("_Background")]
    public bool Background
    {
        get => Logic?._background ?? _background ?? true;
        set => _background = value;
    }

    public RocketLeagueGoalExplosionProperties()
    {
    }

    public RocketLeagueGoalExplosionProperties(bool arg = false) : base(arg)
    {
    }

    public override void Default()
    {
        base.Default();
        _PrimaryColor = Color.FromArgb(125,0,0,0);
        _showEnemyGoalExplosion = true;
        _showFriendlyGoalExplosion = true;
        _background = true;
    } 
}

public class RocketLeagueGoalExplosionLayerHandler() : LayerHandler<RocketLeagueGoalExplosionProperties>("Goal Explosion")
{
    private int _previousOwnTeamGoals;
    private int _previousOpponentGoals;

    private readonly AnimationTrack[] _tracks =
    [
        new AnimationTrack("Goal Explosion Track 0", 1.0f),
        new AnimationTrack("Goal Explosion Track 1", 1.0f, 0.5f),
        new AnimationTrack("Goal Explosion Track 2", 1.0f, 1.0f),
        new AnimationTrack("Goal Explosion Track 3", 1.0f, 1.5f),
        new AnimationTrack("Goal Explosion Track 4", 1.0f, 2.0f)
    ];

    private long _currentTime;

    private float _goalEffectKeyframe;
    private const float GoalEffectAnimationTime = 3.0f;

    private bool _showAnimationExplosion;

    public override EffectLayer Render(IGameState gameState)
    {
        var previousTime = _currentTime;
        _currentTime = Time.GetMillisecondsSinceEpoch();

        var goalExplosionMix = new AnimationMix();

        if (gameState is not GameStateRocketLeague state)
            return EffectLayer.EmptyLayer;

        if (state.Game.Status == RLStatus.Undefined)
            return EffectLayer.EmptyLayer;

        if (state.YourTeam.Goals == -1 || state.OpponentTeam.Goals == -1 || _previousOwnTeamGoals > state.YourTeam.Goals || _previousOpponentGoals > state.OpponentTeam.Goals)
        {
            //reset goals when game ends
            _previousOwnTeamGoals = 0;
            _previousOpponentGoals = 0;
        }

        if (state.YourTeam.Goals > _previousOwnTeamGoals)//keep track of goals even if we dont play the animation
        {
            _previousOwnTeamGoals = state.YourTeam.Goals;
            if (Properties.ShowFriendlyGoalExplosion && state.ColorsValid())
            {
                var playerColor = state.YourTeam.TeamColor;
                SetTracks(playerColor);
                goalExplosionMix.Clear();
                _showAnimationExplosion = true;
            }
        }

        if(state.OpponentTeam.Goals > _previousOpponentGoals)
        {
            _previousOpponentGoals = state.OpponentTeam.Goals;
            if (Properties.ShowEnemyGoalExplosion && state.ColorsValid())
            {
                var opponentColor = state.OpponentTeam.TeamColor;
                SetTracks(opponentColor);
                goalExplosionMix.Clear();
                _showAnimationExplosion = true;
            }
        }

        if (!_showAnimationExplosion) return EffectLayer;

        if(Properties.Background)
            EffectLayer.FillOver(Properties.PrimaryColor);

        goalExplosionMix = new AnimationMix(_tracks);

        EffectLayer.Clear();
        var graphics = EffectLayer.GetGraphics();
        goalExplosionMix.Draw(graphics, _goalEffectKeyframe);
        _goalEffectKeyframe += (_currentTime - previousTime) / 1000.0f;

        if (_goalEffectKeyframe >= GoalEffectAnimationTime)
        {
            _showAnimationExplosion = false;
            _goalEffectKeyframe = 0;
        }
        return EffectLayer;
    }

    protected override UserControl CreateControl()
    {
        return new Control_RocketLeagueGoalExplosionLayer(this);
    }

    private void SetTracks(Color playerColor)
    {
        foreach (var track in _tracks)
        {
            track.SetFrame(
                0.0f, 
                new AnimationCircle(
                    (int)(Effects.Canvas.WidthCenter * 0.9),
                    Effects.Canvas.HeightCenter,
                    0, 
                    playerColor,
                    4)
            );

            track.SetFrame(
                1.0f,
                new AnimationCircle(
                    (int)(Effects.Canvas.WidthCenter * 0.9),
                    Effects.Canvas.HeightCenter, 
                    Effects.Canvas.BiggestSize / 2.0f, 
                    playerColor,
                    4)
            );
        }
    }
}