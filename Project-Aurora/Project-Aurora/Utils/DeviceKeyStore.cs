using System;
using System.Collections;
using System.Collections.Generic;
using AuroraRgb.EffectsEngine;
using Common;
using Common.Devices;

namespace AuroraRgb.Utils;

public sealed class DeviceKeyStore : IDictionary<DeviceKeys, SimpleColor>
{
    public readonly SimpleColor[] ColorArray = new SimpleColor[Effects.MaxDeviceId];
    private readonly bool[] _keyExists = new bool[Effects.MaxDeviceId];

    public ICollection<DeviceKeys> Keys => Enum.GetValues<DeviceKeys>();
    public ICollection<SimpleColor> Values => ColorArray;

    public int Count => Effects.MaxDeviceId;
    public bool IsReadOnly => false;

    public IEnumerator<KeyValuePair<DeviceKeys, SimpleColor>> GetEnumerator()
    {
        for (var i = 0; i < Effects.MaxDeviceId; i++)
        {
            if (_keyExists[i])
                yield return new KeyValuePair<DeviceKeys, SimpleColor>((DeviceKeys)i, ColorArray[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<DeviceKeys, SimpleColor> item)
    {
        var index = GetEnumHash(item.Key);
        _keyExists[index] = true;
        ColorArray[index] = item.Value;
    }

    public void Clear()
    {
        Array.Clear(_keyExists, 0, _keyExists.Length);
        Array.Clear(ColorArray, 0, ColorArray.Length);
    }

    public bool Contains(KeyValuePair<DeviceKeys, SimpleColor> item)
    {
        var index = GetEnumHash(item.Key);
        return _keyExists[index];
    }

    public void CopyTo(KeyValuePair<DeviceKeys, SimpleColor>[] array, int arrayIndex)
    {
        foreach (var kvp in this)
        {
            array[arrayIndex++] = kvp;
        }
    }

    public bool Remove(KeyValuePair<DeviceKeys, SimpleColor> item)
    {
        var exists = Contains(item);
        var index = GetEnumHash(item.Key);
        _keyExists[index] = false;
        return exists;
    }
    
    public void Add(DeviceKeys key, SimpleColor value)
    {
        var index = GetEnumHash(key);
        _keyExists[index] = true;
        ColorArray[index] = value;
    }

    public bool ContainsKey(DeviceKeys key)
    {
        var index = GetEnumHash(key);
        return _keyExists[index];
    }

    public bool Remove(DeviceKeys key)
    {
        var exists = ContainsKey(key);
        var index = GetEnumHash(key);
        _keyExists[index] = false;
        return exists;
    }

    public bool TryGetValue(DeviceKeys key, out SimpleColor value)
    {
        var index = GetEnumHash(key);
        if (index < 0)
        {
            value = SimpleColor.Black;
            return true;
        }
        
        if (_keyExists[index])
        {
            value = ColorArray[index];
            return true;
        }
        value = SimpleColor.Transparent;
        return false;
    }

    public SimpleColor this[DeviceKeys key]
    {
        get
        {
            var index = GetEnumHash(key);
            return ColorArray[index];
        }
        set
        {
            var index = GetEnumHash(key);
            _keyExists[index] = true;
            ColorArray[index] = value;
        }
    }

    private static int GetEnumHash(Enum obj)
    {
        return Convert.ToInt32(obj);
    }
}