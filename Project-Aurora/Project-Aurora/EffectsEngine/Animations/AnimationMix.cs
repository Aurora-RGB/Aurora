﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AuroraRgb.Bitmaps;
using Newtonsoft.Json;

namespace AuroraRgb.EffectsEngine.Animations;

public sealed class AnimationMix: IEquatable<AnimationMix>
{
    [JsonProperty]
    private readonly IDictionary<string, AnimationTrack> _tracks;

    /// <summary>
    /// When true, will remove Animation tracks that no longer have any animations.
    /// </summary>
    [JsonProperty("_automatically_remove_complete")]
    private bool _automaticallyRemoveComplete;

    public AnimationMix()
    {
        _tracks = new ConcurrentDictionary<string, AnimationTrack>();
    }

    public AnimationMix(IEnumerable<AnimationTrack> tracks): this()
    {
        foreach (var track in tracks)
            AddTrack(track);
    }

    [JsonConstructor]
    public AnimationMix(IDictionary<string, AnimationTrack>? tracks, bool automaticallyRemoveComplete)
    {
        _tracks = tracks == null ? new ConcurrentDictionary<string, AnimationTrack>() : new ConcurrentDictionary<string, AnimationTrack>(tracks);
        _automaticallyRemoveComplete = automaticallyRemoveComplete;
    }

    public AnimationMix SetAutoRemove(bool value)
    {
        _automaticallyRemoveComplete = value;

        return this;
    }

    public AnimationMix AddTrack(AnimationTrack? track)
    {
        if (track == null) return this;
        if (_tracks.ContainsKey(track.GetName()))
            _tracks[track.GetName()] = track;
        else
            _tracks.TryAdd(track.GetName(), track);

        return this;
    }

    private void RemoveTrack(string trackName)
    {
        _tracks.Remove(trackName, out _);
    }

    public bool ContainsTrack(string trackName)
    {
        return _tracks.ContainsKey(trackName);
    }

    public float GetDuration()
    {
        return _tracks.Select(track => track.Value.GetShift() + track.Value.AnimationDuration)
            .Prepend(0.0f)
            .Max();
    }

    public ConcurrentDictionary<string, AnimationTrack> GetTracks()
    {
        return (ConcurrentDictionary<string, AnimationTrack>)_tracks;
    }

    public bool AnyActiveTracksAt(float time)
    {
        return _tracks.Any(track => track.Value.ContainsAnimationAt(time));
    }

    public void Draw(IAuroraBitmap g, float time, PointF offset = default)
    {
        foreach (var track in _tracks)
        {
            if (track.Value.ContainsAnimationAt(time))
            {
                var frame = track.Value.GetFrame(time);
                frame.SetOffset(offset);
                frame.Draw(g);
            }
            else if (_automaticallyRemoveComplete)
            {
                RemoveTrack(track.Key);
            }
        }
    }

    public void Clear()
    {
        _tracks.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((AnimationMix) obj);
    }

    public bool Equals(AnimationMix? p)
    {
        return p != null &&
               _tracks.Equals(p._tracks) &&
               _automaticallyRemoveComplete == p._automaticallyRemoveComplete;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + _tracks.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "AnimationMix: [ Count: " + _tracks.Count + " ]";
    }
}