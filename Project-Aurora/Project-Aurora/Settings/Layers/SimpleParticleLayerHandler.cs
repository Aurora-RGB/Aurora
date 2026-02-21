using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.Bitmaps.GdiPlus;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers {

    /// <summary>
    /// Properties for the particle layer handler that handles "simple" particles. Simple particles are ones that just support velocity,
    /// acceleration and drag and whose alive state is determined by their life span.<para />
    /// This class can be overriden and the subclass should pass itself as the <typeparamref name="TSelf"/> parameter.
    /// </summary>
    public partial class SimpleParticleLayerProperties<TSelf> : ParticleLayerPropertiesBase<TSelf> where TSelf : SimpleParticleLayerProperties<TSelf> {

        // Override the default key sequence property so that we can make it trigger a property change notification
        [LogicOverridable("Spawn region")]
        public override KeySequence _Sequence { get; set; }

        private float? _minSpawnTime;
        [JsonProperty("_MinSpawnTime")]
        [LogicOverridable]
        public float MinSpawnTime
        {
            get => Logic?._MinSpawnTime ?? _minSpawnTime ?? .5f;
            set => _minSpawnTime = value;
        }

        private float? _maxSpawnTime;
        [JsonProperty("_MaxSpawnTime")]
        [LogicOverridable] 
        public float MaxSpawnTime
        {
            get => Logic?._MaxSpawnTime ?? _maxSpawnTime ?? 1f;
            set => _maxSpawnTime = value;
        }

        private int? _minSpawnAmount;
        [JsonProperty("_MinSpawnAmount")]
        [LogicOverridable] 
        public int MinSpawnAmount
        {
            get => Logic?._MinSpawnAmount ?? _minSpawnAmount ?? 1;
            set => _minSpawnAmount = value;
        }

        private int? _maxSpawnAmount;
        [JsonProperty("_MaxSpawnAmount")]
        [LogicOverridable] 
        public int MaxSpawnAmount
        {
            get => Logic?._MaxSpawnAmount ?? _maxSpawnAmount ?? 1;
            set => _maxSpawnAmount = value;
        }

        private float? _minInitialVelocityX;
        [JsonProperty("_MinInitialVelocityX")]
        [LogicOverridable] 
        public float MinInitialVelocityX
        {
            get => Logic?._MinInitialVelocityX ?? _minInitialVelocityX ?? 0f;
            set => _minInitialVelocityX = value;
        }

        private float? _maxInitialVelocityX;
        [JsonProperty("_MaxInitialVelocityX")]
        [LogicOverridable] 
        public float MaxInitialVelocityX
        {
            get => Logic?._MaxInitialVelocityX ?? _maxInitialVelocityX ?? 0f;
            set => _maxInitialVelocityX = value;
        }

        private float? _minInitialVelocityY;
        [JsonProperty("_MinInitialVelocityY")]
        [LogicOverridable] 
        public float MinInitialVelocityY
        {
            get => Logic?._MinInitialVelocityY ?? _minInitialVelocityY ?? 0f;
            set => _minInitialVelocityY = value;
        }

        private float? _maxInitialVelocityY;
        [JsonProperty("_MaxInitialVelocityY")]
        [LogicOverridable] 
        public float MaxInitialVelocityY
        {
            get => Logic?._MaxInitialVelocityY ?? _maxInitialVelocityY ?? 0f;
            set => _maxInitialVelocityY = value;
        }

        private float? _minLifetime;
        [JsonProperty("_MinLifetime")]
        [LogicOverridable] 
        public float MinLifetime
        {
            get => Logic?._MinLifetime ?? _minLifetime ?? 3f;
            set => _minLifetime = value;
        }

        private float? _maxLifetime;
        [JsonProperty("_MaxLifetime")]
        [LogicOverridable] 
        public float MaxLifetime
        {
            get => Logic?._MaxLifetime ?? _maxLifetime ?? 3f;
            set => _maxLifetime = value;
        }

        private float? _accelerationX;
        [JsonProperty("_AccelerationX")]
        [LogicOverridable] 
        public float AccelerationX
        {
            get => Logic?._AccelerationX ?? _accelerationX ?? 0f;
            set => _accelerationX = value;
        }

        private float? _accelerationY;
        [JsonProperty("_AccelerationY")]
        [LogicOverridable] 
        public float AccelerationY
        {
            get => Logic?._AccelerationY ?? _accelerationY ?? -1f;
            set => _accelerationY = value;
        }

        private float? _dragX;
        [JsonProperty("_DragX")]
        [LogicOverridable] 
        public float DragX
        {
            get => Logic?._DragX ?? _dragX ?? 0;
            set => _dragX = value;
        }

        private float? _dragY;
        [JsonProperty("_DragY")]
        [LogicOverridable] 
        public float DragY
        {
            get => Logic?._DragY ?? _dragY ?? 0;
            set => _dragY = value;
        }

        private float? _minSize;
        // The smallest initial size of the particlespublic float? _MinSize { get; set; }
        [JsonProperty("_MinSize")]
        [LogicOverridable] 
        public float MinSize
        {
            get => Logic?._MinSize ?? _minSize ?? 6;
            set => _minSize = value;
        }

        private float? _maxSize;
        // The largest initial size of the particlespublic float? _MaxSize { get; set; }
        [JsonProperty("_MaxSize")]
        [LogicOverridable] 
        public float MaxSize
        {
            get => Logic?._MaxSize ?? _maxSize ?? 6;
            set => _maxSize = value;
        }

        private float? _deltaSize;
        [JsonProperty("_DeltaSize")]
        [LogicOverridable] 
        public float DeltaSize
        {
            get => Logic?._DeltaSize ?? _deltaSize ?? 0;
            set => _deltaSize = value;
        }

        // Where the particles will spawn from
        private ParticleSpawnLocations? _spawnLocation;
        [JsonProperty("_SpawnLocation")]
        [LogicOverridable] 
        public ParticleSpawnLocations SpawnLocation
        {
            get => Logic?._SpawnLocation ?? _spawnLocation ?? ParticleSpawnLocations.BottomEdge;
            set => _spawnLocation = value;
        }

        // The color gradient stops for the particle. Note this is sorted by offset when set using _ParticleColorStops. Not using a linear brush here because:
        //   1) there are multithreading issues when trying to access a Media brush's gradient collection since it belongs to the UI thread
        //   2) We don't actually need the gradient as a brush since we're not drawing particles as gradients, only a solid color based on their lifetime, so we only need to access the color stops
        private ColorStopCollection? _particleColorStops;
        [JsonProperty("_ParticleColorStops")]
        [LogicOverridable] 
        public ColorStopCollection ParticleColorStops
        {
            get => Logic?._ParticleColorStops ?? _particleColorStops ?? DefaultParticleColor;
            set => _particleColorStops = value;
        }

        // An override proxy for setting the particle color stops
        [JsonIgnore, LogicOverridable("Color over time")] public EffectBrush ParticleBrush {
            get => new EffectBrush(_particleColorStops.ToMediaBrush());
            set => _particleColorStops = value == null ? null : ColorStopCollection.FromMediaBrush(value.GetMediaBrush());
        }

        private static readonly ColorStopCollection DefaultParticleColor = new()
        {
            {0f, Color.White },
            {1f,  Color.FromArgb(0, Color.White) }
        };

        public override void Default() {
            base.Default();
            _spawnLocation = ParticleSpawnLocations.BottomEdge;
            _particleColorStops = DefaultParticleColor;
            _minSpawnTime = _maxSpawnTime = .1f;
            _minSpawnAmount = _maxSpawnAmount = 1;
            _minLifetime = 0; _maxLifetime = 2;
            _minInitialVelocityX = _maxInitialVelocityX = 0;
            _minInitialVelocityY = _maxInitialVelocityY = -1;
            _accelerationX = 0;
            _accelerationY = .5f;
            _dragX = 0;
            _dragY = 0;
            _minSize = 6; _maxSize = 6;
            _deltaSize = 0;
            _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
        }
    }

    /// <summary>
    /// Properties for the particle layer handler that handles "simple" particles. Simple particles are ones that just support velocity,
    /// acceleration and drag and whose alive state is determined by their life span.
    /// </summary>
    public partial class SimpleParticleLayerProperties : SimpleParticleLayerProperties<SimpleParticleLayerProperties>;

    /// <summary>
    /// Layer handler for the simple particles. It uses time-based spawning and despawning logic.
    /// </summary>
    [LayerHandlerMeta(Name = "Particle", IsDefault = true)]
    [LogicOverrideIgnoreProperty(nameof(LayerHandlerProperties._PrimaryColor))]
    public class SimpleParticleLayerHandler : ParticleLayerHandlerBase<SimpleParticle, SimpleParticleLayerProperties> {

        private double _nextSpawnInterval; // How many seconds until next set of particle spawns
        
        protected override UserControl CreateControl() => new Control_ParticleLayer(this);

        protected override void SpawnParticles(double deltaTime)
        {
            if (!Properties.SpawningEnabled) return;
            _nextSpawnInterval -= deltaTime;

            if (_nextSpawnInterval >= 0) return;

            var count = Rnd.Next(Properties.MinSpawnAmount, Properties.MaxSpawnAmount + 1);
            for (var i = 0; i < count; i++)
                SpawnParticle();
            _nextSpawnInterval = RandomBetween(Properties.MinSpawnTime, Properties.MaxSpawnTime);
        }
    }

    /// <summary>
    /// Particle definition that handles "simple" particles. Simple particles are ones that just support velocity,
    /// acceleration and drag and whose alive state is determined by their life span.
    /// </summary>
    public sealed class SimpleParticle : IParticle<SimpleParticleLayerProperties> {

        private static readonly Random Rnd = new();

        private readonly SingleColorBrush _solidBrush = new(SimpleColor.Transparent);
        
        private PointF _position;

        private float PositionX
        {
            get => _position.X;
            set => _position.X = value;
        }

        private float PositionY
        {
            get => _position.Y;
            set => _position.Y = value;
        }
        private float VelocityX { get; set; }
        private float VelocityY { get; set; }
        private float Size { get; set; }
        private double Lifetime { get; set; }
        private double MaxLifetime { get; }

        /// <summary>
        /// When the particle is created, randomise it's position and velocity according to the properties and canvas size.
        /// </summary>
        public SimpleParticle(SimpleParticleLayerProperties properties) {
            var ar = properties.Sequence.GetAffectedRegion();
            MaxLifetime = (float)(Rnd.NextDouble() * (properties.MaxLifetime - properties.MinLifetime) + properties.MinLifetime);
            VelocityX = (float)(Rnd.NextDouble() * (properties.MaxInitialVelocityX - properties.MinInitialVelocityX) + properties.MinInitialVelocityX);
            VelocityY = (float)(Rnd.NextDouble() * (properties.MaxInitialVelocityY - properties.MinInitialVelocityY) + properties.MinInitialVelocityY);
            PositionX
                = properties.SpawnLocation == ParticleSpawnLocations.LeftEdge ? 0 // For left edge, X should start at 0
                : properties.SpawnLocation == ParticleSpawnLocations.RightEdge ? Effects.Canvas.Width // For right edge, X should start at maximum width
                : properties.SpawnLocation == ParticleSpawnLocations.Region ? ar.Left + (float)(Rnd.NextDouble() * ar.Width)// For region, randomly choose X in region
                : (float)(Rnd.NextDouble() * Effects.Canvas.Width); // For top, bottom or random, randomly choose an X value
            PositionY
                = properties.SpawnLocation == ParticleSpawnLocations.TopEdge ? 0 // For top edge, Y should start at 0
                : properties.SpawnLocation == ParticleSpawnLocations.BottomEdge ? Effects.Canvas.Height // For bottom edge, Y should start at maximum height
                : properties.SpawnLocation == ParticleSpawnLocations.Region ? ar.Top + (float)(Rnd.NextDouble() * ar.Height)// For region, randomly choose Y in region
                : (float)(Rnd.NextDouble() * Effects.Canvas.Height); // For left, right or random, randomly choose a Y value
            Size = (float)(Rnd.NextDouble() * (properties.MaxSize - properties.MinSize)) + properties.MinSize;
        }

        /// <summary>
        /// The particle's life is based on it's <see cref="Lifetime"/> and <see cref="MaxLifetime"/> properties.
        /// </summary>
        public bool IsAlive() => Lifetime < MaxLifetime;

        public void Render(GdiBitmap gfx, SimpleParticleLayerProperties properties) {
            var color = properties.ParticleColorStops.GetColorAt((float)(Lifetime / MaxLifetime));
            _solidBrush.Color = (SimpleColor)color;

            var s2 = Size / 2;
            var rectangleF = new RectangleF(PositionX - s2, PositionY - s2, Size, Size);
            gfx.FillEllipse(_solidBrush, rectangleF);
        }

        /// <summary>
        /// Update the velocity of the particle based on the acceleration and drag. Then, update the position based on the velocity.
        /// </summary>
        public void Update(double deltaTime, SimpleParticleLayerProperties properties) {
            Lifetime += deltaTime;
            VelocityX += (float)(properties.AccelerationX * deltaTime);
            VelocityY += (float)(properties.AccelerationY * deltaTime);
            VelocityX *= (float)Math.Pow(1 - properties.DragX, deltaTime); // By powering the drag to the deltaTime, we ensure that the results are fairly consistent over different time deltas.
            VelocityY *= (float)Math.Pow(1 - properties.DragY, deltaTime); // Doing it once over a second won't be 100% the same as doing it twice over a second if acceleration is present,
                                                                           // but it should be close enough that it won't be noticed under most cicrumstances
            PositionX += VelocityX;
            PositionY += VelocityY;
            Size += (float)(properties.DeltaSize * deltaTime);
        }

        public void Dispose()
        {
            _solidBrush.Dispose();
        }
    }


    /// <summary>
    /// An enum dictating possible spawn locations for the particles.
    /// </summary>
    public enum ParticleSpawnLocations {
        [Description("Top edge")] TopEdge,
        [Description("Right edge")] RightEdge,
        [Description("Bottom edge")] BottomEdge,
        [Description("Left edge")] LeftEdge,
        Region,
        Random
    }


    /// <summary>
    /// Class that holds some preset configurations for the ParticleLayerProperties class.
    /// </summary>
    public static class ParticleLayerPresets {
        public static ReadOnlyDictionary<string, Action<SimpleParticleLayerProperties>> Presets { get; } = new ReadOnlyDictionary<string, Action<SimpleParticleLayerProperties>>(
            new Dictionary<string, Action<SimpleParticleLayerProperties>> {
                { "Fire", p => {
                    p.SpawnLocation = ParticleSpawnLocations.BottomEdge;
                    p.ParticleColorStops = new ColorStopCollection {
                        { 0f, Color.Yellow },
                        { 0.6f, Color.FromArgb(128, Color.Red) },
                        { 1f, Color.FromArgb(0, Color.Black) }
                    };
                    p.MinSpawnTime = p.MaxSpawnTime = .05f;
                    p.MinSpawnAmount = 4; p.MaxSpawnAmount = 6;
                    p.MinLifetime = .5f; p.MaxLifetime = 2;
                    p.MinInitialVelocityX = p.MaxInitialVelocityX = 0;
                    p.MinInitialVelocityY = -1.3f; p.MaxInitialVelocityY = -0.8f;
                    p.AccelerationX = 0;
                    p.AccelerationY = 0.5f;
                    p.MinSize = 8; p.MaxSize = 12;
                    p.DeltaSize = -4;
                } },
                { "Matrix", p => {
                    p.SpawnLocation = ParticleSpawnLocations.TopEdge;
                    p.ParticleColorStops = new ColorStopCollection {
                        { 0f, Color.FromArgb(0,255,0) },
                        { 1f, Color.FromArgb(0,255,0) }
                    };
                    p.MinSpawnTime = .1f; p.MaxSpawnTime = .2f;
                    p.MinSpawnAmount = 1; p.MaxSpawnAmount = 2;
                    p.MinLifetime = p.MaxLifetime = 1;
                    p.MinInitialVelocityX = p.MaxInitialVelocityX = 0;
                    p.MinInitialVelocityY = p.MaxInitialVelocityY = 3;
                    p.AccelerationX = 0;
                    p.AccelerationY = 0;
                    p.MinSize = 6; p.MaxSize = 6;
                    p.DeltaSize = 0;
                } },
                { "Rain", p => {
                    p.SpawnLocation = ParticleSpawnLocations.TopEdge;
                    p.ParticleColorStops = new ColorStopCollection {
                        { 0f, Color.Cyan },
                        { 1f, Color.Cyan }
                    };
                    p.MinSpawnTime = .1f; p.MaxSpawnTime = .2f;
                    p.MinSpawnAmount = 1; p.MaxSpawnAmount = 2;
                    p.MinLifetime = p.MaxLifetime = 1;
                    p.MinInitialVelocityX = p.MaxInitialVelocityX = 0;
                    p.MinInitialVelocityY = p.MaxInitialVelocityY = 3;
                    p.AccelerationX = 0;
                    p.AccelerationY = 0;
                    p.MinSize = 2;
                    p.MaxSize = 4;
                    p.DeltaSize = 0;
                } }
            }
        );
    }
}
