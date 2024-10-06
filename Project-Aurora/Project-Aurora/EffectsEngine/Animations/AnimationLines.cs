using System;
using System.Collections.Generic;
using AuroraRgb.Bitmaps;
using Newtonsoft.Json;

namespace AuroraRgb.EffectsEngine.Animations;

public class AnimationLines : AnimationFrame
{
    [JsonProperty]
    private List<AnimationLine> _lines;

    public AnimationLines(IEnumerable<AnimationLine> lines, float duration = 0.0f)
    {
        _lines = [..lines];
        _duration = duration;
    }

    public override void Draw(IAuroraBitmap g)
    {
        foreach (var line in _lines)
            line.Draw(g);
    }

    public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
    {
        if (!(otherAnim is AnimationLines animationLines))
        {
            throw new FormatException("Cannot blend with another type");
        }

        if (_lines.Count != animationLines._lines.Count)
        {
            throw new NotImplementedException();
        }

        amount = GetTransitionValue(amount);

        List<AnimationLine> newlines = [];

        for (var line_i = 0; line_i < _lines.Count; line_i++)
            newlines.Add(_lines[line_i].BlendWith(animationLines._lines[line_i], amount) as AnimationLine);

        return new AnimationLines(newlines.ToArray());
    }

    public override AnimationFrame GetCopy()
    {
        var newlines = new List<AnimationLine>(_lines.Count);

        foreach (var line in _lines)
        {
            newlines.Add(line.GetCopy() as AnimationLine);
        }

        return new AnimationLines(newlines.ToArray(), _duration).SetAngle(_angle).SetTransitionType(_transitionType);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AnimationLines)obj);
    }

    public bool Equals(AnimationLines p)
    {
        return _lines.Equals(p._lines) &&
               _duration.Equals(p._duration);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + _lines.GetHashCode();
            hash = hash * 23 + _duration.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "AnimationLines [ Lines: " + _lines.Count + " ]";
    }
}