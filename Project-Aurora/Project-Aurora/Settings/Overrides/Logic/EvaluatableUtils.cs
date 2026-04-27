using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Class that provides a lookup for the default Evaluatable for a particular type.
/// </summary>
public static class EvaluatableDefaults {

    private static readonly Dictionary<Type, Type> DefaultsMap = new()
    {
        [typeof(bool)] = typeof(BooleanConstant),
        [typeof(int)] = typeof(NumberConstant),
        [typeof(long)] = typeof(NumberConstant),
        [typeof(float)] = typeof(NumberConstant),
        [typeof(double)] = typeof(NumberConstant),
        [typeof(string)] = typeof(StringConstant)
    };

    public static Evaluatable<T> Get<T>() => (Evaluatable<T>)Get(typeof(T));

    public static IEvaluatable Get(Type t) {
        if (!DefaultsMap.TryGetValue(t, out var @default))
            throw new ArgumentException($"Type '{t.Name}' does not have a default evaluatable type.");
        return (IEvaluatable)Activator.CreateInstance(@default);
    }
}

/// <summary>
/// Helper classes for the Evaluatables.
/// </summary>
public static class EvaluatableHelpers {
    /// <summary>Attempts to get an evaluatable from the supplied data object. Will return true/false indicating if data is of correct format
    /// (an <see cref="Evaluatable{T}"/> where T matches the given type. If the eval type is null, no type check is performed, the returned
    /// evaluatable may be of any sub-type.</summary>
    internal static bool TryGetData(IDataObject @do,
        [MaybeNullWhen(false)] out IEvaluatable evaluatable,
        [MaybeNullWhen(false)] out Control_EvaluatablePresenter source,
        Type? evalType)
    {
        if (@do.GetData(@do.GetFormats()
                .FirstOrDefault(x => x != "SourcePresenter")) is IEvaluatable data &&
            (evalType == null || data.GetType().GetGenericParentTypes(typeof(Evaluatable<>))[0] == evalType)) {
            evaluatable = data;
            source = (Control_EvaluatablePresenter)@do.GetData("SourcePresenter")!;
            return true;
        }
        evaluatable = null;
        source = null;
        return false;
    }
}