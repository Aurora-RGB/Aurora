using System;
using System.Collections.Generic;

namespace AuroraRgb.Utils;

public enum CurveFunction
{
    Unset,
    Linear,
    Squared,
    Cubed,
    SquareRoot,
    Sine,
    SineSquared,
}

public static class CurveFunctions
{
    private static readonly Dictionary<CurveFunction, Func<double, double>> Funcs = new()
    {
        { CurveFunction.Unset , Unset },
        { CurveFunction.Linear, Linear },
        { CurveFunction.Squared, Squared },
        { CurveFunction.Cubed, Cubed },
        { CurveFunction.SquareRoot, SquareRoot },
        { CurveFunction.Sine, Sine },
        { CurveFunction.SineSquared, SineSquared }
    };

    public static readonly IReadOnlyDictionary<CurveFunction, Func<double, double>> Functions = Funcs;

    private static double Unset(double x)
    {
        return 0;
    }

    private static double Linear(double x)
    {
        return x;
    }

    private static double Squared(double x)
    {
        return Math.Pow(x, 2);
    }

    private static double Cubed(double x)
    {
        return Math.Pow(x, 3);
    }

    private static double SquareRoot(double x)
    {
        return Math.Sqrt(x);
    }

    private static double Sine(double x)
    {
        return Math.Sin(x * Math.PI / 2);
    }

    private static double SineSquared(double x)
    {
        return Math.Pow(Math.Sin(x * Math.PI / 2), 2);
    }
}