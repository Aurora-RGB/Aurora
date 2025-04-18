﻿using System;
using System.ComponentModel;
using AuroraRgb.Utils;
using JetBrains.Annotations;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Simple attribute that can be added to conditions to add metadata to them and register them as conditions.
/// Unregistered conditions will still work, but they will not be shown in the dropdown list when editing layer visibility conditions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class EvaluatableAttribute : Attribute {

    /// <param name="name">The name of the condition (will appear in the dropdown list).</param>
    public EvaluatableAttribute(string name, EvaluatableCategory category = EvaluatableCategory.Misc) {
        Name = name;
        Category = category;
    }

    /// <summary>The name of the condition (will appear in the dropdown list).</summary>
    public string Name { get; }

    /// <summary>The category this condition belongs to (items will be grouped by this in the dropdown list).</summary>
    public EvaluatableCategory Category { get; }

    /// <summary>Gets the description of the category as a string.</summary>
    public string CategoryStr => Category.GetDescription();
}

public enum EvaluatableCategory {
    [Description("Logic")] Logic,
    [Description("State Variable")] State,
    [Description("Input")] Input,
    [Description("Misc.")] Misc,
    [Description("Maths")] Maths,
    [Description("String")] String,
    [Description("Global Variable")] Global,
}