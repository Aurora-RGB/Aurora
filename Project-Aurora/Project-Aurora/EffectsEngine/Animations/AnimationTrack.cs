using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AuroraRgb.EffectsEngine.Animations;

public sealed class AnimationTrack
{
    [JsonProperty("_animations")]
    private readonly IDictionary<float, AnimationFrame> _animations;

    [JsonProperty("_animationDuration")]
    private float _animationDuration;

    [JsonProperty("_track_name")]
    private string _trackName;

    [JsonProperty("_shift")]
    private float _shift;

    private bool _supportedTypeIdentified;
    private Type _supportedAnimationType = typeof(AnimationFrame);

    [JsonIgnore]
    private readonly Dictionary<int, AnimationFrame> _blendCache = new();

    public Type SupportedAnimationType
    {
        get
        {
            if (!_supportedTypeIdentified)
            {
                if (_animations.Count > 0)
                {
                    _supportedAnimationType = _animations.Values.ToArray()[0].GetType();
                    _supportedTypeIdentified = true;
                }
            }

            return _supportedAnimationType;
        }
    }

    public float AnimationDuration => _animationDuration;

    [JsonConstructor]
    public AnimationTrack(IDictionary<float, AnimationFrame>? animations, float animationDuration, string trackName, float shift)
    {
        _animations = new ConcurrentDictionary<float, AnimationFrame>(animations ?? new Dictionary<float, AnimationFrame>());
        _animationDuration = animationDuration;
        _trackName = trackName;
        _shift = shift;
    }

    public AnimationTrack(string trackName, float animationDuration, float shift = 0.0f)
    {
        _animations = new ConcurrentDictionary<float, AnimationFrame>();
        _trackName = trackName;
        _animationDuration = animationDuration;
        _shift = shift;
    }

    public AnimationTrack SetName(string name)
    {
        _trackName = name;

        return this;
    }

    public string GetName()
    {
        return _trackName;
    }

    public AnimationTrack SetShift(float shift)
    {
        _shift = shift;

        return this;
    }

    public float GetShift()
    {
        return _shift;
    }

    private float NormalizeTime(float time)
    {
        //Shift
        return time - _shift;
    }

    public bool ContainsAnimationAt(float time)
    {
        time = NormalizeTime(time);

        return time <= _animationDuration && _animations.Count != 0;
    }

    public AnimationTrack SetFrame(float time, AnimationFrame animframe)
    {
        //One can retype the animation track by removing all frames
        if (_animations.Count == 0)
        {
            _supportedAnimationType = animframe.GetType();
            _supportedTypeIdentified = true;
        }

        if (_supportedAnimationType == animframe.GetType())
        {
            if (_animations.Count != 0)
            {
                var closeValues = GetCloseValues(time);

                if (closeValues.Item1 + _animations[closeValues.Item1]._duration > time && time > closeValues.Item1)
                    _animations[closeValues.Item1].SetDuration(time - closeValues.Item1);
            }

            _animations[time] = animframe;
        }

        UpdateDuration();

        return this;
    }

    public AnimationTrack RemoveFrame(float time)
    {
        foreach (var kvp in _animations)
        {
            if (kvp.Key == time)
            {
                _animations.Remove(kvp.Key, out _);
                break;
            }
        }

        UpdateDuration();
        return this;
    }

    public AnimationTrack Clear()
    {
        _animations.Clear();
        _blendCache.Clear();

        UpdateDuration();
        return this;
    }

    public AnimationFrame GetFrame(float time)
    {
        if (!ContainsAnimationAt(time))
            return (AnimationFrame)Activator.CreateInstance(SupportedAnimationType);

        time = NormalizeTime(time);

        if (time > _animationDuration || _animations.Count == 0)
            return (AnimationFrame)Activator.CreateInstance(SupportedAnimationType);

        var (lower, higher) = GetCloseValues(time);

        if (!_animations.ContainsKey(lower) || lower > time)
            return new AnimationFrame();

        //The time value is exact
        if (Math.Abs(lower - higher) < 0.1)
            return _animations[lower];
        if (lower + _animations[lower]._duration > time)
            return _animations[lower];
        if (_animations.TryGetValue(lower, out var value) && _animations.TryGetValue(higher, out var animation))
        {
            var roundedTime = (int)Math.Round(time * 100);
            if (_blendCache.TryGetValue(roundedTime, out var blend))
            {
                return blend;
            }

            var blendAmount = (time - (lower + value._duration)) / (double)(higher - (lower + value._duration));
            blend = value.BlendWith(animation, blendAmount);
            _blendCache.Add(roundedTime, blend);
            return blend;
        }

        return (AnimationFrame)Activator.CreateInstance(SupportedAnimationType);
    }

    public ConcurrentDictionary<float, AnimationFrame> GetAnimations()
    {
        return (ConcurrentDictionary<float, AnimationFrame>)_animations;
    }

    private Tuple<float, float> GetCloseValues(float time)
    {
        var closestLower = _animations.Keys.Min();
        var closestHigher = _animationDuration;

        foreach (var kvp in _animations)
        {
            if (Math.Abs(kvp.Key - time) < 0.001)
                return new Tuple<float, float>(time, time);

            if (kvp.Key > time && kvp.Key < closestHigher)
                closestHigher = kvp.Key;

            if (kvp.Key < time && kvp.Key > closestLower)
                closestLower = kvp.Key;
        }

        return new Tuple<float, float>(closestLower, closestHigher);
    }

    private void UpdateDuration()
    {
        _blendCache.Clear();
        if (_animations.Count > 0)
        {
            var max = _animations.Keys.Max();
            _animationDuration = max + _animations[max].Duration;
        }
        else
            _animationDuration = 0;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AnimationTrack)obj);
    }

    public bool Equals(AnimationTrack p)
    {
        return _trackName.Equals(p._trackName) &&
               _animationDuration == p._animationDuration &&
               _shift == p._shift &&
               _animations.Equals(p._animations);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + _trackName.GetHashCode();
            hash = hash * 23 + _animationDuration.GetHashCode();
            hash = hash * 23 + _shift.GetHashCode();
            hash = hash * 23 + _animations.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "AnimationTrack: " + _trackName + " { Frames: " + _animations.Count + " Duration: " + _animationDuration + " sec. Shift: " + _shift + " sec. }";
    }
}