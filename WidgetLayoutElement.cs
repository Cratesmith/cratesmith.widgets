using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
    public struct WidgetLayoutElement : IEquatable<WidgetLayoutElement>
    {
        public Optional<bool>  ignoreLayout;
        public Optional<float> minWidth;
        public Optional<float> minHeight;
        public Optional<float> preferredWidth;
        public Optional<float> preferredHeight;
        public Optional<float> flexibleWidth;
        public Optional<float> flexibleHeight;

        public WidgetLayoutElement AsDefault()
        {
            return new WidgetLayoutElement()
            {
                ignoreLayout = ignoreLayout.AsDefault(),
                minWidth = minWidth.AsDefault(),
                minHeight = minHeight.AsDefault(),
                preferredWidth = preferredWidth.AsDefault(),
                preferredHeight = preferredHeight.AsDefault(),
                flexibleWidth = flexibleWidth.AsDefault(),
                flexibleHeight = flexibleHeight.AsDefault(),
            };
        }
        
        public bool Apply(LayoutElement to, LayoutElement defaults, bool usesTypePrefab)
        {
            bool changed = false;
            var _ignoreLayout = ignoreLayout.GetValue(defaults.ignoreLayout,usesTypePrefab);
            var _minWidth = minWidth.GetValue(defaults.minWidth,usesTypePrefab);
            var _minHeight = minHeight.GetValue(defaults.minHeight,usesTypePrefab);
            var _preferredWidth = preferredWidth.GetValue(defaults.preferredWidth,usesTypePrefab);
            var _preferredHeight = preferredHeight.GetValue(defaults.preferredHeight,usesTypePrefab);
            var _flexibleWidth = flexibleWidth.GetValue(defaults.flexibleWidth,usesTypePrefab);
            var _flexibleHeight = flexibleHeight.GetValue(defaults.flexibleHeight,usesTypePrefab);
            if(to.ignoreLayout!=_ignoreLayout) {to.ignoreLayout = _ignoreLayout;changed=true;}
            if(!Mathf.Approximately(to.minWidth,_minWidth)) {to.minWidth = _minWidth;changed=true;}
            if(!Mathf.Approximately(to.minHeight,_minHeight)) {to.minHeight = _minHeight;changed=true;}
            if(!Mathf.Approximately(to.preferredWidth,_preferredWidth)) {to.preferredWidth = _preferredWidth;changed=true;}
            if(!Mathf.Approximately(to.preferredHeight,_preferredHeight)) {to.preferredHeight = _preferredHeight;changed=true;}
            if(!Mathf.Approximately(to.flexibleWidth,_flexibleWidth)) {to.flexibleWidth = _flexibleWidth;changed=true;}
            if(!Mathf.Approximately(to.flexibleHeight,_flexibleHeight)) {to.flexibleHeight = _flexibleHeight;changed=true;}
            return changed;
        }

        public static WidgetLayoutElement IgnoreLayout()
        {
            return new WidgetLayoutElement()
            {
                ignoreLayout = true
            };
        }

        public bool Equals(WidgetLayoutElement other)
        {
            return ignoreLayout.Equals(other.ignoreLayout) && minWidth.Equals(other.minWidth) && minHeight.Equals(other.minHeight) && preferredWidth.Equals(other.preferredWidth) && preferredHeight.Equals(other.preferredHeight) && flexibleWidth.Equals(other.flexibleWidth) && flexibleHeight.Equals(other.flexibleHeight);
        }

        public override bool Equals(object obj)
        {
            return obj is WidgetLayoutElement other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ignoreLayout.GetHashCode();
                hashCode = (hashCode * 397) ^ minWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ minHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ preferredWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ preferredHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ flexibleWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ flexibleHeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(WidgetLayoutElement left, WidgetLayoutElement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WidgetLayoutElement left, WidgetLayoutElement right)
        {
            return !left.Equals(right);
        }
    }
}