using System;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
    public struct WidgetContentSizeFitter : IEquatable<WidgetContentSizeFitter>
    {
        public Optional<ContentSizeFitter.FitMode> horizontalFit;
        public Optional<ContentSizeFitter.FitMode> verticalFit;

        public WidgetContentSizeFitter AsDefault()
        {
            return new WidgetContentSizeFitter()
            {
                horizontalFit = horizontalFit.AsDefault(),
                verticalFit = verticalFit.AsDefault()
            };
        }
        
        public bool Apply(ContentSizeFitter to, ContentSizeFitter defaults, bool usesTypePrefab)
        {
            var _horizontalFit = horizontalFit.GetValue(defaults.horizontalFit,usesTypePrefab);
            var _verticalFit = verticalFit.GetValue(defaults.verticalFit,usesTypePrefab);
            bool changed = false;
            if(to.horizontalFit!=horizontalFit) {to.horizontalFit = _horizontalFit; changed=true;}
            if(to.verticalFit!=verticalFit) {to.verticalFit = _verticalFit; changed=true;}
            return changed;
        }

        public static WidgetContentSizeFitter Preferred(bool horizontal=true, bool vertical=true)
        {
            return new WidgetContentSizeFitter()
            {
                horizontalFit = horizontal
                    ? ContentSizeFitter.FitMode.PreferredSize
                    : ContentSizeFitter.FitMode.Unconstrained,
                verticalFit = vertical
                    ? ContentSizeFitter.FitMode.PreferredSize
                    : ContentSizeFitter.FitMode.Unconstrained,
            };
        }

        public bool Equals(WidgetContentSizeFitter other)
        {
            return horizontalFit.Equals(other.horizontalFit) && verticalFit.Equals(other.verticalFit);
        }

        public override bool Equals(object obj)
        {
            return obj is WidgetContentSizeFitter other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (horizontalFit.GetHashCode() * 397) ^ verticalFit.GetHashCode();
            }
        }

        public static bool operator ==(WidgetContentSizeFitter left, WidgetContentSizeFitter right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WidgetContentSizeFitter left, WidgetContentSizeFitter right)
        {
            return !left.Equals(right);
        }
    }
}