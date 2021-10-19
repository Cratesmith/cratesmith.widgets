using System;
using UnityEngine;

[Serializable]
public struct WidgetRectOffset : IEquatable<RectOffset>, IEquatable<WidgetRectOffset>
{
    public int left;
    public int right;
    public int top;
    public int bottom;
    public WidgetRectOffset(int _left, int _right, int _top, int _bottom)
    {
        left = _left;
        right = _right;
        top = _top;
        bottom = _bottom;
    }

    public void ApplyTo(RectOffset to)
    {
        to.left = left;
        to.right = right;
        to.top = top;
        to.bottom = bottom;
    }
    
    public static implicit operator WidgetRectOffset(in RectOffset x)
    {
        return new WidgetRectOffset(x.left, x.right, x.top, x.bottom);
    }

    public bool Equals(RectOffset other)
    {
        return left == other.left 
               && right==other.right
               && bottom == other.bottom
               && top == other.top;
    }

    public bool Equals(WidgetRectOffset other)
    {
        return left == other.left && right == other.right && top == other.top && bottom == other.bottom;
    }

    public override bool Equals(object obj)
    {
        return obj is WidgetRectOffset other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = left;
            hashCode = (hashCode * 397) ^ right;
            hashCode = (hashCode * 397) ^ top;
            hashCode = (hashCode * 397) ^ bottom;
            return hashCode;
        }
    }

    public static bool operator ==(WidgetRectOffset left, WidgetRectOffset right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(WidgetRectOffset left, WidgetRectOffset right)
    {
        return !left.Equals(right);
    }
}