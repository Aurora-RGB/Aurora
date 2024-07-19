using System.Collections.Generic;
using System.Drawing;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Utils;
using DK = Common.Devices.DeviceKeys;

namespace AuroraRgb.Profiles.StardewValley
{
    public class StardewValleyProfile : ApplicationProfile
    {
        public override void Reset()
        {
            base.Reset();

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Title/Loading", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.DarkCyan
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanOr(new List<Evaluatable<bool>> { new BooleanGSIEnum("Game/Status", GSI.Nodes.GameStatus.TitleScreen), new BooleanGSIEnum("Game/Status", GSI.Nodes.GameStatus.Loading) }))
                    .SetLookupTable("_PrimaryColor",new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 255, 210, 132),
                                new BooleanGSIEnum("Game/Status", GSI.Nodes.GameStatus.Loading))
                    )
                ),

                new Layer("Damage", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = new AnimationMix(new[] {
                            new AnimationTrack("Filled Rectangle Track", 0.5f)
                                .SetFrame(0, new AnimationFilledRectangle(0, 0, 800, 300, Color.Red))
                                .SetFrame(0.5f, new AnimationFilledRectangle(0, 0, 800, 300, Color.FromArgb(0, 255, 0, 0)))
                        }),
                        _AnimationDuration = 0.5f,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnLow,
                        TriggerPath = new VariablePath("Player/Health/Current"),
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Health Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        VariablePath = new VariablePath("Player/Health/Current"),
                        MaxVariablePath = new VariablePath("Player/Health/Max"),
                        _PrimaryColor = Color.Lime,
                        SecondaryColor = Color.Red,
                        _Sequence = new KeySequence(new[] {
                            DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE, DK.SIX, DK.SEVEN, DK.EIGHT, DK.NINE, DK.ZERO
                        })
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/Health/BarActive"))
                ),

                new Layer("Energy Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        VariablePath = new VariablePath("Player/Energy/Current"),
                        MaxVariablePath = new VariablePath("Player/Energy/Max"),
                        _PrimaryColor = Color.Yellow,
                        SecondaryColor = Color.Red,
                        _Sequence = new KeySequence(new[] {
                            DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                        })
                    }
                }),

                new Layer("Debris/Fall", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        SpawnLocation = ParticleSpawnLocations.TopEdge,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0f, Color.FromArgb(255, 255, 131, 65) },
                            { 1f, Color.FromArgb(255, 255, 131, 65) }
                        },
                        MinSpawnTime = .7f,
                        MaxSpawnTime = .9f,
                        MinSpawnAmount = 1,
                        MaxSpawnAmount = 2,
                        MinLifetime = 5,
                        MaxLifetime = 5,
                        MinInitialVelocityX = -0.5f,
                        MaxInitialVelocityX = -0.2f,
                        MinInitialVelocityY = 0.3f,
                        MaxInitialVelocityY = 0.3f,
                        AccelerationX = 0,
                        AccelerationY = 0,
                        MinSize = 7,
                        MaxSize = 7,
                        DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanAnd(new List<Evaluatable<bool>> { new BooleanGSIBoolean("World/Weather/IsDebrisWeather"), new BooleanGSIBoolean("Player/IsOutdoor"), new BooleanNot(new BooleanGSIEnum("Player/Location", GSI.Nodes.PlayerNode.Locations.Desert)) }))
                ),

                new Layer("Raining", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        SpawnLocation = ParticleSpawnLocations.TopEdge,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0f, Color.Blue },
                            { 1f, Color.Blue }
                        },
                        MinSpawnTime = .1f,
                        MaxSpawnTime = .2f,
                        MinSpawnAmount = 2,
                        MaxSpawnAmount = 2,
                        MinLifetime = 1,
                        MaxLifetime = 1,
                        MinInitialVelocityX = -1.5f,
                        MaxInitialVelocityX = -2,
                        MinInitialVelocityY =2,
                        MaxInitialVelocityY = 2,
                        AccelerationX = 0,
                        AccelerationY = 0,
                        MinSize = 5,
                        MaxSize = 7,
                        DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanAnd(new List<Evaluatable<bool>> { new BooleanGSIBoolean("World/Weather/IsRaining"), new BooleanGSIBoolean("Player/IsOutdoor"), new BooleanNot(new BooleanGSIEnum("Player/Location", GSI.Nodes.PlayerNode.Locations.Desert)) }))
                ),

                new Layer("Snowing", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        SpawnLocation = ParticleSpawnLocations.TopEdge,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0f, Color.White },
                            { 1f, Color.White }
                        },
                        MinSpawnTime = .1f,
                        MaxSpawnTime = .2f,
                        MinSpawnAmount = 1,
                        MaxSpawnAmount = 2,
                        MinLifetime = 4,
                        MaxLifetime = 4,
                        MinInitialVelocityX = 0,
                        MaxInitialVelocityX = 0.5f,
                        MinInitialVelocityY =0.5f,
                        MaxInitialVelocityY = 0.5f,
                        AccelerationX = 0,
                        AccelerationY = 0,
                        MinSize = 5,
                        MaxSize = 5,
                        DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanAnd(new List<Evaluatable<bool>> {
                        new BooleanGSIBoolean("World/Weather/IsSnowing"), new BooleanGSIBoolean("Player/IsOutdoor"), new BooleanNot(new BooleanGSIEnum("Player/Location", GSI.Nodes.PlayerNode.Locations.Desert)) }))
                ),

                new Layer("Background/Season", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.Transparent
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor",new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 252, 39, 185),
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Spring))
                        .AddEntry(Color.FromArgb(255, 63, 217, 4),
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Summer))
                        .AddEntry(Color.FromArgb(255, 153, 61, 4),
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Fall))
                        .AddEntry(Color.FromArgb(255, 0, 110, 255),
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Winter))
                    )
                ),

                new Layer("Background/Time", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.Transparent
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor",new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.Violet,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Sunrise))
                        .AddEntry(Color.Coral,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Morning))
                        .AddEntry(Color.Yellow,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Daytime))
                        .AddEntry(Color.Orange,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Evening))
                        .AddEntry(Color.OrangeRed,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Twilight))
                        .AddEntry(Color.SlateBlue,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Night))
                        .AddEntry(Color.MediumBlue,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Midnight))
                    )
                ),
            };
        }

    }
}
