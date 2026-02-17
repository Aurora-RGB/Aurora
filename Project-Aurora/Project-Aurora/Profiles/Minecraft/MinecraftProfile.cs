using System.Collections.Generic;
using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Minecraft.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Utils;
using DK = Common.Devices.DeviceKeys;

namespace AuroraRgb.Profiles.Minecraft;

public class MinecraftProfile : ApplicationProfile
{

    public override void Reset()
    {
        base.Reset();

        // Keys that do something and should be highlighted in a static color
        DK[] controlKeys = [DK.W, DK.A, DK.S, DK.D, DK.E, DK.SPACE, DK.LEFT_SHIFT, DK.LEFT_CONTROL];

        Layers =
        [
            new Layer("Controls Assistant Layer", new MinecraftKeyConflictLayerHandler()),

            new Layer("Health Bar", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        VariablePath = new VariablePath("Player/Health"),
                        MaxVariablePath = new VariablePath("Player/HealthMax"),
                        PrimaryColor = Color.Red,
                        SecondaryColor = Color.Transparent,
                        Sequence = new KeySequence([
                            DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA, DK.PERIOD, DK.FORWARD_SLASH
                        ])
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(Layer.Enabled), new BooleanGSIBoolean("Player/InGame"))
                    .SetLookupTable(nameof(LayerHandlerProperties.PrimaryColor), new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 210, 0), new BooleanGSIBoolean("Player/PlayerEffects/HasAbsorption"))
                        .AddEntry(Color.FromArgb(240, 75, 100), new BooleanGSIBoolean("Player/PlayerEffects/HasRegeneration"))
                        .AddEntry(Color.FromArgb(145, 160, 30), new BooleanGSIBoolean("Player/PlayerEffects/HasPoison"))
                        .AddEntry(Color.FromArgb(70, 5, 5), new BooleanGSIBoolean("Player/PlayerEffects/HasWither"))
                    )
            ),

            new Layer("Experience Bar", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        VariablePath = new VariablePath("Player/Experience"),
                        MaxVariablePath = new VariablePath("Player/ExperienceMax"),
                        PrimaryColor = Color.FromArgb(255, 255, 0),
                        SecondaryColor = Color.Transparent,
                        Sequence = new KeySequence([
                            DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                        ])
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(Layer.Enabled), new BooleanGSIBoolean("Player/InGame"))
            ),

            new Layer("Water Controls", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        PrimaryColor = Color.Blue,
                        Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(Layer.Enabled), new BooleanAnd([
                        new BooleanGSIBoolean("Player/IsInWater"), new BooleanGSIBoolean("Player/InGame")
                    ]))
            ),

            new Layer("Sneaking Controls", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        PrimaryColor = Color.FromArgb(45, 90, 90),
                        Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(Layer.Enabled), new BooleanAnd([
                        new BooleanGSIBoolean("Player/IsSneaking"), new BooleanGSIBoolean("Player/InGame")
                    ]))
            ),

            new Layer("Horse Controls", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        PrimaryColor = Color.Orange,
                        Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(Layer.Enabled), new BooleanAnd(new List<BooleanGSIBoolean>([
                            new BooleanGSIBoolean("Player/IsRidingHorse"), new BooleanGSIBoolean("Player/InGame")
                        ]
                    )))
            ),

            new Layer("Player Controls", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        PrimaryColor = Color.White,
                        Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(Layer.Enabled), new BooleanGSIBoolean("Player/InGame"))
            ),

            new Layer("On Fire", new SimpleParticleLayerHandler
                {
                    Properties = new SimpleParticleLayerProperties
                    {
                        SpawnLocation = ParticleSpawnLocations.BottomEdge,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0f, Color.Orange },
                            { 0.6f, Color.Red },
                            { 1f, Color.Black }
                        },
                        MinSpawnTime = 0.05f,
                        MaxSpawnTime = 0.05f,
                        MinSpawnAmount = 8,
                        MaxSpawnAmount = 10,
                        MinLifetime = 0.5f,
                        MaxLifetime = 2f,
                        MinInitialVelocityX = 0,
                        MaxInitialVelocityX = 0,
                        MinInitialVelocityY = -5f,
                        MaxInitialVelocityY = -0.8f,
                        AccelerationX = 0f,
                        AccelerationY = 0.5f,
                        MinSize = 8,
                        MaxSize = 12,
                        DeltaSize = -4,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(SimpleParticleLayerProperties.SpawningEnabled), new BooleanGSIBoolean("Player/IsBurning"))
            ),

            new Layer("Raining", new SimpleParticleLayerHandler
                {
                    Properties = new SimpleParticleLayerProperties
                    {
                        SpawnLocation = ParticleSpawnLocations.TopEdge,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0f, Color.Cyan },
                            { 1f, Color.Cyan }
                        },
                        MinSpawnTime = .1f,
                        MaxSpawnTime = .2f,
                        MinSpawnAmount = 1,
                        MaxSpawnAmount = 2,
                        MinLifetime = 1,
                        MaxLifetime = 1,
                        MinInitialVelocityX = 0,
                        MaxInitialVelocityX = 0,
                        MinInitialVelocityY = 3,
                        MaxInitialVelocityY = 3,
                        AccelerationX = 0,
                        AccelerationY = 0,
                        MinSize = 2,
                        MaxSize = 4,
                        DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean(nameof(SimpleParticleLayerProperties.SpawningEnabled), new BooleanGSIBoolean("World/IsRaining"))
            ),

            new Layer("Grass Block Top", new MinecraftBackgroundLayerHandler
                {
                    Properties = new MinecraftBackgroundLayerHandlerProperties
                    {
                        PrimaryColor = Color.FromArgb(44, 168, 32),
                        SecondaryColor = Color.FromArgb(30, 80, 25),
                        Sequence = new KeySequence(new FreeFormObject(-20, -60, 900, 128))
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable(nameof(LayerHandlerProperties.PrimaryColor), new OverrideLookupTableBuilder<Color>()
                            .AddEntry(Color.FromArgb(125, 42, 123), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", 1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The End
                            .AddEntry(Color.FromArgb(255, 183, 0), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", -1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The Nether
                    )
                    .SetLookupTable("SecondaryColor", new OverrideLookupTableBuilder<Color>()
                            .AddEntry(Color.FromArgb(49, 0, 59), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", 1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The End
                            .AddEntry(Color.FromArgb(87, 83, 0), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", -1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The Nether
                    )
            ),

            new Layer("Grass Block Side", new MinecraftBackgroundLayerHandler
                {
                    Properties = new MinecraftBackgroundLayerHandlerProperties
                    {
                        PrimaryColor = Color.FromArgb(125, 70, 15), //(102, 59, 20),
                        SecondaryColor = Color.FromArgb(80, 50, 25)
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable(nameof(LayerHandlerProperties.PrimaryColor), new OverrideLookupTableBuilder<Color>()
                            .AddEntry(Color.FromArgb(209, 232, 80), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", 1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The End
                            .AddEntry(Color.FromArgb(184, 26, 0), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", -1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The Nether
                    )
                    .SetLookupTable("SecondaryColor", new OverrideLookupTableBuilder<Color>()
                            .AddEntry(Color.FromArgb(107, 102, 49), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", 1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The End
                            .AddEntry(Color.FromArgb(59, 8, 0), new BooleanAnd([
                                new BooleanGSINumeric("World/DimensionId", -1),
                                new BooleanGSIBoolean("Player/InGame")
                            ])) //The Nether
                    )
            )
        ];
    }
}
