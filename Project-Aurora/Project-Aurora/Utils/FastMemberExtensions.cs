using System;
using System.Collections;
using System.Globalization;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using FastMember;

namespace AuroraRgb.Utils;

public static class FastMemberExtensions {

    /// <summary>
    /// Takes a path to a property (e.g. "Property/NestedProperty") and attempts to resolve it into a value within the context of this object.
    /// </summary>
    public static object? ResolvePropertyPath(this object target, VariablePath path)
    {
        if (target is IGameState gameState && gameState.PropertyMap.TryGetValue(path.GsiPath, out var getter))
        {
            return getter.Invoke(gameState);
        }

        return ResolveWithReflection(target, path);
    }

    [Obsolete("Shouldn't be needed when NewtonsoftGameState is gone")]
    private static object? ResolveWithReflection(object target, VariablePath path)
    {
        var pathParts = path.GsiPath.Split('/');
        var curObj = target;
        try
        {
            foreach (var part in pathParts)
            {
                // If this is an enumerable and the part is a valid number, get the nth item of that enumerable
                if (curObj is IEnumerable e && int.TryParse(part, CultureInfo.InvariantCulture, out var index))
                    curObj = e.ElementAtIndex(index);

                // Otherwise if this is any other object, use FastMember to access the relevant property/field.
                else
                    curObj = curObj is IGameState gs ? gs.LazyObjectAccessor.Value[part] : ObjectAccessor.Create(curObj)[part];
            }

            return curObj; // If we got here, there is a valid object at this path, return it.
        }
        catch (ArgumentOutOfRangeException)
        {
            // Thrown if ObjectAccessor attempts to get a field/property that doesn't exist
        }
        catch (IndexOutOfRangeException)
        {
            // Thrown if IEnumerable.ElementAtIndex tries to go out of bounds 
        }
        return null;
    }
}