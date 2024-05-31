using System.Linq.Expressions;

namespace Common.Utils;

/// <summary>
/// Class to cast to type <see cref="T"/>
/// https://stackoverflow.com/a/23391746/13320838
/// </summary>
/// <typeparam name="T">Target type</typeparam>
public static class CastTo<T>
{
    /// <summary>
    /// Casts <see cref="S"/> to <see cref="T"/>.
    /// This does not cause boxing for value types.
    /// Useful in generic methods.
    /// </summary>
    /// <typeparam name="S">Source type to cast from. Usually a generic type.</typeparam>
    public static T From<S>(S s)
    {
        return Cache<S>.Caster(s);
    }    

    private static class Cache<S>
    {
        public static readonly Func<S, T> Caster = Get();

        private static Func<S, T> Get()
        {
            var p = Expression.Parameter(typeof(S));
            var c = Expression.ConvertChecked(p, typeof(T));
            return Expression.Lambda<Func<S, T>>(c, p).Compile();
        }
    }
}