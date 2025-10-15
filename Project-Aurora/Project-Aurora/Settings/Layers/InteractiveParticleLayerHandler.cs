using System.Collections.Concurrent;
using System.Threading.Tasks;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Inputs;
using Common.Devices;

namespace AuroraRgb.Settings.Layers;

[LayerHandlerMeta(Name = "Particle (Interactive)", IsDefault = true)]
public sealed class InteractiveParticleLayerHandler : SimpleParticleLayerHandler {

    private readonly ConcurrentQueue<DeviceKeys> _awaitingKeys = new();

    protected override async Task Initialize()
    {
        await base.Initialize();

        (await InputsModule.InputEvents).KeyDown += KeyDown;
    }

    private void KeyDown(object? sender, KeyboardKeyEventArgs e) {
        _awaitingKeys.Enqueue(e.GetDeviceKey());
    }

    protected override void SpawnParticles(double dt)
    {
        var particleCount = _awaitingKeys.Count;
        for (var n = 0; n < particleCount; n++)
        {
            var count = Rnd.Next(Properties.MinSpawnAmount, Properties.MaxSpawnAmount);
            for (var i = 0; i < count; i++)
                SpawnParticle();
        }
        _awaitingKeys.Clear();
    }

    public override void Dispose()
    {
        InputsModule.InputEvents.Result.KeyDown -= KeyDown;
        base.Dispose();
    }
}