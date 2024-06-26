﻿using System.Drawing;

namespace AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

public class Team_RocketLeague : AutoJsonNode<Team_RocketLeague>
{
    /// <summary>
    /// Name of the team. Usually Blue or Orange, but can be different in custom games and for clan teams
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of goals the team scored
    /// </summary>
    public int Goals { get; set; }

    /// <summary>
    /// Red value of the teams color (0-1)
    /// </summary>
    public float Red { get; set; }

    /// <summary>
    /// Green value of the teams color (0-1)
    /// </summary>
    public float Green { get; set; }

    /// <summary>
    /// Blue value of the teams color (0-1)
    /// </summary>
    public float Blue { get; set; }

    internal Team_RocketLeague(string json) : base(json) { }

    public Color TeamColor
    {
        get =>
            Color.FromArgb((int)(Red * 255.0f),
                (int)(Green * 255.0f),
                (int)(Blue * 255.0f));
        set
        {
            Red = value.R / 255.0f;
            Green = value.G / 255.0f;
            Blue = value.B / 255.0f;
        }
    }
}