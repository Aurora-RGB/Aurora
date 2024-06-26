﻿using AuroraRgb.Profiles.Generic;

namespace AuroraRgb.Profiles.Osu.GSI;

public class GameState_Osu : NewtonsoftGameState {

    public ProviderNode Provider => NodeFor<ProviderNode>("Provider");

    public GameNode Game => NodeFor<GameNode>("game");

    public GameState_Osu()
    { }
    public GameState_Osu(string jsonString) : base(jsonString) { }
}

public class GameNode : AutoJsonNode<GameNode> {
    [AutoJsonPropertyName("status")]
    public OsuStatus StatusEnum;
    public string Status => StatusEnum.ToString(); // Only here for legacy reasons - don't wanna break any profiles that may now depend on this
    public OsuPlayMode PlayMode;
    public float HP;
    public float Accuracy;
    public int Combo;
    public int Count50;
    public int Count100;
    public int Count200;
    public int Count300;
    public int CountKatu;
    public int CountGeki;
    public int CountMiss;

    internal GameNode(string json) : base(json) { }
}