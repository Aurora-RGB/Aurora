﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using AuroraRgb.Bitmaps;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Overrides;
using JetBrains.Annotations;
using Newtonsoft.Json;
using static System.Linq.Expressions.Expression;

namespace AuroraRgb.Settings.Layers {

    /// <summary>
    /// The base properties for the base particle layer.
    /// </summary>
    public partial class ParticleLayerPropertiesBase<TSelf> : LayerHandlerProperties where TSelf : ParticleLayerPropertiesBase<TSelf> {
        
        //This allows the particle system to be turned off without disabling it (thereby not hiding already spawned particles).
        private bool? _spawningEnabled;
         
        [JsonProperty("_SpawningEnabled")]
        [LogicOverridable("Enable Particle Spawning")]
        public bool SpawningEnabled
        {
            get => Logic?._SpawningEnabled ?? _spawningEnabled ?? true;
            set => _spawningEnabled = value;
        }

        public override void Default() {
            base.Default();
            _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
        }
    }

    /// <summary>
    /// The base particle layer handler which manages and renders a collection of particles.<para/>
    /// The behaviour of the particle is determined by the particle implementation.
    /// </summary>
    /// <typeparam name="TParticle"></typeparam>
    /// <typeparam name="TProperties"></typeparam>
    public abstract class ParticleLayerHandlerBase<TParticle, TProperties> : LayerHandler<TProperties, BitmapEffectLayer>, INotifyRender
        where TParticle : IParticle<TProperties>
        where TProperties : ParticleLayerPropertiesBase<TProperties> {

        private static readonly Func<TProperties, TParticle> SpawnLambda;

        protected static readonly Random Rnd = new();
        private readonly Stopwatch _stopwatch = new(); // Stopwatch to determine time difference since last render
        private readonly List<TParticle> _particles = new(); // All the currently active "alive" particles

        public event EventHandler<IAuroraBitmap> LayerRender; // Fires whenever the layer is rendered

        static ParticleLayerHandlerBase()
        {
            var particleCtor = typeof(TParticle).GetConstructor(new[] {typeof(TProperties)});
            var tpropParam = Parameter(typeof(TProperties));
            SpawnLambda = Lambda<Func<TProperties, TParticle>>(
                New(particleCtor, tpropParam),
                tpropParam
            ).Compile();
        }

        protected ParticleLayerHandlerBase() : base("Particle Layer")
        {
        }

        protected override abstract UserControl CreateControl();

        public override EffectLayer Render(IGameState gameState) {

            // Get elapsed time since last render
            var dt = _stopwatch.ElapsedMilliseconds / 1000d;
            _stopwatch.Restart();

            // Update and render all particles
            var gfx = EffectLayer.GetGraphics(); {
                EffectLayer.Clear();
                foreach (var particle in _particles) {
                    particle.Update(dt, Properties);
                    if (particle.IsAlive())
                        particle.Render(gfx, Properties);
                }
            }

            // Spawn new particles if required
            SpawnParticles(dt);

            // Remove any particles that have expired
            _particles.RemoveAll(p => !p.IsAlive());

            // Call the render event
            LayerRender?.Invoke(this, EffectLayer.GetBitmap());
            
            EffectLayer.OnlyInclude(Properties.Sequence);

            return EffectLayer;
        }

        /// <summary>
        /// Performs the logic to spawn the particles. Determines whether or not the particles should be spawned and
        /// how many, then calls <see cref="SpawnParticle"/> for each.
        /// </summary>
        /// <param name="deltaTime">The time (in seconds) since the last call.</param>
        protected abstract void SpawnParticles(double deltaTime);

        /// <summary>
        /// Creates a new particle and returns it. By default, calls a constructor on <typeparamref name="TParticle"/> with a single <typeparamref name="TParticle"/> parameter.
        /// </summary>
        protected virtual void SpawnParticle() => _particles.Add(SpawnLambda(Properties));

        /// <summary>
        /// Picks a random floating point number between the two given numbers.
        /// </summary>
        protected static float RandomBetween(float a, float b) => (float)(Rnd.NextDouble() * (b - a) + a);
    }


    /// <summary>
    /// Interface for the particle data class.
    /// </summary>
    /// <typeparam name="TProperties">The layer handler properties type that is passed to the particle.</typeparam>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
    public interface IParticle<TProperties> : IDisposable where TProperties : ParticleLayerPropertiesBase<TProperties> {

        /// <summary>Updates the data of the particle (e.g. position, velocity).</summary>
        /// <param name="deltaTime">The time (in seconds) since the last update.</param>
        void Update(double deltaTime, TProperties properties);

        /// <summary>Renders the particle to the given graphics context.</summary>
        void Render(IAuroraBitmap gfx, TProperties properties);

        /// <summary>Determines if the particle is alive. A particle that is not alive will be removed from the canvas.</summary>
        bool IsAlive();
    }
}
