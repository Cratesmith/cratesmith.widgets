using System;
using UnityEngine;

namespace com.cratesmith.widgets
{
    public struct WidgetBasicLayout : IEquatable<WidgetBasicLayout>
    {
        public Optional<bool>               isVertical;
        public Optional<WidgetRectOffset>   padding;
        public Optional<float>              spacing;
        public Optional<TextAnchor>         childAlignment;
        public Optional<bool>               childControlWidth;
        public Optional<bool>               childControlHeight;
        public Optional<bool>               childForceExpandWidth;
        public Optional<bool>               childForceExpandHeight;
        public Optional<bool>               reverseArrangement;
        public Optional<bool>               childScaleHeight;
        public Optional<bool>               childScaleWidth;

        public static WidgetBasicLayout Vertical(in Optional<TextAnchor> _childAlignment=default)
        {
            return new WidgetBasicLayout()
            {
                isVertical = true,
                childAlignment = _childAlignment,
            };
        }
        
        public static WidgetBasicLayout Horizontal(in Optional<TextAnchor> _childAlignment=default)
        {
            return new WidgetBasicLayout()
            {
                isVertical = false,
                childAlignment = _childAlignment
            };
        }

        public WidgetBasicLayout AsDefault()
        {
            return new WidgetBasicLayout()
            {
                isVertical = this.isVertical.AsDefault(),
                padding = this.padding.AsDefault(),
                spacing = this.spacing.AsDefault(),
                childAlignment = this.childAlignment.AsDefault(),
                childControlWidth = this.childControlWidth.AsDefault(),
                childControlHeight = this.childControlHeight.AsDefault(),
                childForceExpandWidth = this.childForceExpandWidth.AsDefault(),
                childForceExpandHeight = this.childForceExpandHeight.AsDefault(),
                reverseArrangement = this.reverseArrangement.AsDefault(),
                childScaleHeight = this.childScaleHeight.AsDefault(),
                childScaleWidth = this.childScaleWidth.AsDefault(),
            };
        }
        
        public bool Apply(BasicLayoutGroup to, BasicLayoutGroup defaults, bool usesTypePrefab)
        {
            bool changed = false;
            padding.GetValue(defaults.padding,usesTypePrefab).ApplyTo(to.padding);
            var _isVertical = isVertical.GetValue(defaults.isVertical,usesTypePrefab);
            var _spacing = spacing.GetValue(defaults.spacing,usesTypePrefab);
            var _childAlignment = childAlignment.GetValue(defaults.childAlignment,usesTypePrefab);
            var _childControlWidth = childControlWidth.GetValue(defaults.childControlWidth,usesTypePrefab);
            var _childControlHeight = childControlHeight.GetValue(defaults.childControlHeight,usesTypePrefab);
            var _childForceExpandWidth = childForceExpandWidth.GetValue(defaults.childForceExpandWidth,usesTypePrefab);
            var _childForceExpandHeight = childForceExpandHeight.GetValue(defaults.childForceExpandHeight,usesTypePrefab);
            var _reverseArrangement = reverseArrangement.GetValue(defaults.reverseArrangement,usesTypePrefab);
            var _childScaleHeight = childScaleHeight.GetValue(defaults.childScaleHeight,usesTypePrefab);
            var _childScaleWidth = childScaleWidth.GetValue(defaults.childScaleWidth,usesTypePrefab);
            if(to.isVertical != _isVertical) {to.isVertical = _isVertical; changed=true;}
            if(to.spacing != _spacing) {to.spacing = _spacing; changed=true;}
            if(to.childAlignment != _childAlignment) {to.childAlignment = _childAlignment; changed=true;}
            if(to.childControlWidth != _childControlWidth) {to.childControlWidth = _childControlWidth; changed=true;}
            if(to.childControlHeight != _childControlHeight) {to.childControlHeight = _childControlHeight; changed=true;}
            if(to.childForceExpandWidth != _childForceExpandWidth) {to.childForceExpandWidth = _childForceExpandWidth; changed=true;}
            if(to.childForceExpandHeight != _childForceExpandHeight) {to.childForceExpandHeight = _childForceExpandHeight; changed=true;}
            if(to.reverseArrangement != _reverseArrangement) {to.reverseArrangement = _reverseArrangement; changed=true;}
            if(to.childScaleHeight != _childScaleHeight) {to.childScaleHeight = _childScaleHeight; changed=true;}
            if(to.childScaleWidth != _childScaleWidth) {to.childScaleWidth = _childScaleWidth; changed=true;}
            return changed;
        }

        public bool Equals(WidgetBasicLayout other)
        {
            return isVertical.Equals(other.isVertical) && padding.Equals(other.padding) && spacing.Equals(other.spacing) && childAlignment.Equals(other.childAlignment) && childControlWidth.Equals(other.childControlWidth) && childControlHeight.Equals(other.childControlHeight) && childForceExpandWidth.Equals(other.childForceExpandWidth) && childForceExpandHeight.Equals(other.childForceExpandHeight) && reverseArrangement.Equals(other.reverseArrangement) && childScaleHeight.Equals(other.childScaleHeight) && childScaleWidth.Equals(other.childScaleWidth);
        }

        public override bool Equals(object obj)
        {
            return obj is WidgetBasicLayout other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = isVertical.GetHashCode();
                hashCode = (hashCode * 397) ^ padding.GetHashCode();
                hashCode = (hashCode * 397) ^ spacing.GetHashCode();
                hashCode = (hashCode * 397) ^ childAlignment.GetHashCode();
                hashCode = (hashCode * 397) ^ childControlWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ childControlHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ childForceExpandWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ childForceExpandHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ reverseArrangement.GetHashCode();
                hashCode = (hashCode * 397) ^ childScaleHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ childScaleWidth.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(WidgetBasicLayout left, WidgetBasicLayout right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WidgetBasicLayout left, WidgetBasicLayout right)
        {
            return !left.Equals(right);
        }
    }
}
