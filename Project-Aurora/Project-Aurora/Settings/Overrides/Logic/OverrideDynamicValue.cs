using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;
using Common;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// The override dynamic logic creates a values dynamically based on some given IEvaluatables.
/// This differs from the lookup table as non-discrete values can be used instead.
/// </summary>
[JsonObject]
public class OverrideDynamicValue : IOverrideLogic
{
    private const string Value = "Value";

    #region Constructors

    /// <summary>
    /// Creates a new OverrideDynamicValue for the specified type of property.
    /// <paramref name="type">The type of property being edited. E.G. for the "Enabled" property, the type will be `typeof(bool)`</paramref>
    /// </summary>
    public OverrideDynamicValue(Type type)
    {
        VarType = type;

        // Create a new set of constructor parameters by taking all the defined values in the typeDynamicDefMap for this type, then creating
        // a new instance of the default IEvaluatable for each parameter. E.G. for a parameter specified as EvaluatableType.Boolean, a new true
        // constant will be put in the constructor parameters dictionary.
        ConstructorParameters = TypeDynamicDefMap.TryGetValue(type, out var value)
            ? value.ConstructorParameters.ToDictionary(dcpd => dcpd.Name, dcpd => EvaluatableDefaults.Get(dcpd.Type))
            : new Dictionary<string, IEvaluatable>();
    }

    /// <summary>
    /// Creates a new OverrideDynamicValue for the specified type of property with an existing set of constructor parameters.
    /// This constructor is the one called by the JSON deserializer so that it doesn't re-create the constructor parameters from the given type.
    /// <paramref name="type">The type of property being edited. E.G. for the "Enabled" property, the type will be `typeof(bool)`</paramref>
    /// <paramref name="constructorParameters">A collection of parameters that will be evaluated to construct the type.</paramref> 
    /// </summary>
    [JsonConstructor]
    public OverrideDynamicValue(Type type, Dictionary<string, IEvaluatable> constructorParameters)
    {
        VarType = type;
        ConstructorParameters = constructorParameters;
    }

    #endregion

    /// <summary>Event for when a property changes.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #region Properties

    /// <summary>The type of variable being handled by the dynamic value logic.</summary>
    public Type VarType
    {
        get => _varType;
        set
        {
            if ((value.IsClass || value.IsValueType) && !value.IsPrimitive)
            {
                _value = new Lazy<object>(() => Instantiator.Constructor(value).Invoke(null));
            }

            _varType = value;
        }
    }

    /// <summary>A dictionary of all parameters to be passed to the constructing method for this dynamic value property.
    /// The key is the name of the variable and the value is the IEvaluatable instance as specified in the typeDynamicDefMap.</summary>
    public Dictionary<string, IEvaluatable> ConstructorParameters { get; set; }

    private Lazy<object> _value = new();
    private Type _varType = null!;

    #endregion

    #region Methods

    /// <summary>
    /// Evaluates the DynamicConstructor logic with the given gamestate.
    /// </summary>
    public object? Evaluate(IGameState gameState)
    {
        if (Creators.TryGetValue(VarType, out var creator))
        {
            return creator(gameState, this);
        }

        return null;
    }

    /// <summary>
    /// Creates the control that is used to edit the IEvaluatables used as parameters for this DynamicValue logic
    /// </summary>
    public Visual GetControl() => TypeDynamicDefMap.ContainsKey(VarType)
        // If this has a valid type (i.e. supported by the dynamic constructor), then create the control and pass in `this` and `application` for context
        ? new Control_OverrideDynamicValue(this)
        // If it is an invalid type, then simply show a red warning message
        : new Label
        {
            Content = "This property type is not supported with the dynamic value editor. Sorry :(", Foreground = Brushes.Red,
            Margin = new System.Windows.Thickness(6)
        };

    #endregion

    internal static readonly IReadOnlyDictionary<Type, Func<IGameState, OverrideDynamicValue, object?>> Creators =
        new Dictionary<Type, Func<IGameState, OverrideDynamicValue, object?>>
        {
            // Boolean
            { typeof(bool), (state, evaluator) => evaluator.ConstructorParameters[Value].Evaluate(state) },

            // Numeric
            { typeof(int), (state, evaluator) => evaluator.ConstructorParameters[Value].Evaluate(state) },
            { typeof(long), (state, evaluator) => evaluator.ConstructorParameters[Value].Evaluate(state) },
            { typeof(float), (state, evaluator) => evaluator.ConstructorParameters[Value].Evaluate(state) },
            { typeof(double), (state, evaluator) => evaluator.ConstructorParameters[Value].Evaluate(state) },

            // Special
            {
                typeof(System.Drawing.Color), (state, evaluator) =>
                {
                    var p = evaluator.ConstructorParameters;
                    return System.Drawing.Color.FromArgb(
                        ToColorComp(p["Alpha"].Evaluate(state)),
                        ToColorComp(p["Red"].Evaluate(state)),
                        ToColorComp(p["Green"].Evaluate(state)),
                        ToColorComp(p["Blue"].Evaluate(state))
                    );
                }
            },
            {
                typeof(KeySequence), (state, evaluator) =>
                {
                    var keySequence = (KeySequence)evaluator._value.Value;
                    var p = evaluator.ConstructorParameters;
                    var freeform = keySequence.Freeform;
                    freeform.X = Convert.ToSingle(p["X"].Evaluate(state));
                    freeform.Y = Convert.ToSingle(p["Y"].Evaluate(state));
                    freeform.Width = Convert.ToSingle(p["Width"].Evaluate(state));
                    freeform.Height = Convert.ToSingle(p["Height"].Evaluate(state));
                    freeform.Angle = Convert.ToSingle(p["Angle"].Evaluate(state));

                    return keySequence;
                }
            },
            {
                typeof(System.Drawing.Rectangle), (state, evaluator) =>
                {
                    var rectangle = (System.Drawing.Rectangle)evaluator._value.Value;
                    var p = evaluator.ConstructorParameters;

                    rectangle.X = Convert.ToInt32(p["X"].Evaluate(state));
                    rectangle.Y = Convert.ToInt32(p["Y"].Evaluate(state));
                    rectangle.Width = Convert.ToInt32(p["Width"].Evaluate(state));
                    rectangle.Height = Convert.ToInt32(p["Height"].Evaluate(state));
                    return rectangle;
                }
            },
            {
                typeof(SimpleColor), (state, evaluator) =>
                {
                    var p = evaluator.ConstructorParameters;

                    var a = Convert.ToByte(p["A"].Evaluate(state));
                    var r = Convert.ToByte(p["R"].Evaluate(state));
                    var g = Convert.ToByte(p["G"].Evaluate(state));
                    var b = Convert.ToByte(p["B"].Evaluate(state));
                    return new SimpleColor(r, g, b, a);
                }
            },
        };

    /// <summary>
    /// Dictionary map that contains a list of all supported types (that can be dynamically constructed) along with their constructor definitions.
    /// Each item in this dictionary represents a type that can be constructed using the dynamic value logic. The DCD contains a list of parameters
    /// that are required for the constructor (including what type they are) and also a constructor function which is passed these RESOLVED
    /// parameters each frame.
    /// </summary>
    internal static readonly IReadOnlyDictionary<Type, DynamicSetterDefinition> TypeDynamicDefMap = new Dictionary<Type, DynamicSetterDefinition>
    {
        // Boolean
        { typeof(bool), new DynamicSetterDefinition([new DynamicPropertyDefinition(Value, typeof(bool))]) },

        // Numeric
        { typeof(int), new DynamicSetterDefinition([new DynamicPropertyDefinition(Value, typeof(double))]) },
        { typeof(long), new DynamicSetterDefinition([new DynamicPropertyDefinition(Value, typeof(double))]) },
        { typeof(float), new DynamicSetterDefinition([new DynamicPropertyDefinition(Value, typeof(double))]) },
        { typeof(double), new DynamicSetterDefinition([new DynamicPropertyDefinition(Value, typeof(double))]) },

        // Special
        {
            typeof(System.Drawing.Color), new DynamicSetterDefinition([
                    new DynamicPropertyDefinition("Alpha", typeof(double), "A value between 0 (transparent) and 1 (opaque) for the transparency of the color."),
                    new DynamicPropertyDefinition("Red", typeof(double), "A value between 0 and 1 for the amount of red in the color."),
                    new DynamicPropertyDefinition("Green", typeof(double), "A value between 0 and 1 for the amount of green in the color."),
                    new DynamicPropertyDefinition("Blue", typeof(double), "A value between 0 and 1 for the amount of blue in the color.")
                ]
            )
        },

        {
            typeof(KeySequence), new DynamicSetterDefinition([
                    new DynamicPropertyDefinition("X", typeof(double)),
                    new DynamicPropertyDefinition("Y", typeof(double)),
                    new DynamicPropertyDefinition("Width", typeof(double)),
                    new DynamicPropertyDefinition("Height", typeof(double)),
                    new DynamicPropertyDefinition("Angle", typeof(double))
                ]
            )
        },
        {
            typeof(System.Drawing.Rectangle), new DynamicSetterDefinition([
                    new DynamicPropertyDefinition("X", typeof(double)),
                    new DynamicPropertyDefinition("Y", typeof(double)),
                    new DynamicPropertyDefinition("Width", typeof(double)),
                    new DynamicPropertyDefinition("Height", typeof(double))
                ]
            )
        }
    };

    #region Dynamic Constructor Helper Methods

    /// <summary>Converts a double object (from 0-1) into a color component (int between 0 and 255).</summary>
    private static int ToColorComp(object c) => double.IsNaN((double)c) ? 0 : Convert.ToInt32(MathUtils.Clamp((double)c, 0, 1) * 255);

    #endregion
}

/// <summary>
/// Struct which defines a constructor that can be used to dynamically create a value using one or more IEvaluatables.
/// </summary>
struct DynamicSetterDefinition(DynamicPropertyDefinition[] parameters)
{
    /// <summary>The type of parameters expected by this constructor method.</summary>
    public readonly DynamicPropertyDefinition[] ConstructorParameters = parameters;
}

struct DynamicPropertyDefinition(string name, Type type, string? description = null)
{
    /// <summary>Parameter name.</summary>
    public readonly string Name = name;

    /// <summary>The type of variable this parameter is.</summary>
    public readonly Type Type = type;

    /// <summary>A simple description of the parameter for the user.</summary>
    public readonly string? Description = description;
}