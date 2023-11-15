﻿using Aurora.Profiles;
using Common.Devices;

namespace Aurora.Settings;

public interface IEffectScript
{
    string? ID { get; }

    VariableRegistry Properties { get; }

    object UpdateLights(VariableRegistry properties, IGameState state = null);
}