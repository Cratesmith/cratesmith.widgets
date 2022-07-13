using System;
using System.Collections.Generic;
using SubModules.com.cratesmith.widgets;
using UnityEngine;

namespace com.cratesmith.widgets
{
    public struct WidgetRectTransform : IEquatable<WidgetRectTransform>
    {
        public Optional<Vector3> localScale;
        public Optional<Vector2> anchorMin;
        public Optional<Vector2> anchorMax;
        public Optional<Vector2> anchoredPosition;
        public Optional<Vector2> sizeDelta;
        public Optional<Vector2> pivot;

        public WidgetRectTransform AsDefault()
        {
            return new WidgetRectTransform()
            {
                localScale = this.localScale.AsDefault(),
                anchorMin = this.anchorMin.AsDefault(),
                anchorMax = this.anchorMax.AsDefault(),
                anchoredPosition = this.anchoredPosition.AsDefault(),
                sizeDelta = this.sizeDelta.AsDefault(),
                pivot = this.pivot.AsDefault()
            };
        }

        public bool Apply(RectTransform to, RectTransform defaults, bool usesTypePrefab)
        {
            bool changed = false;
            var _localScale = localScale.GetValue(defaults.localScale,usesTypePrefab);
            var _anchorMin = anchorMin.GetValue(defaults.anchorMin,usesTypePrefab);
            var _anchorMax = anchorMax.GetValue(defaults.anchorMax,usesTypePrefab);
            var _anchoredPosition = anchoredPosition.GetValue(defaults.anchoredPosition,usesTypePrefab);
            var _sizeDelta = sizeDelta.GetValue(defaults.sizeDelta,usesTypePrefab);
            var _pivot = pivot.GetValue(defaults.pivot,usesTypePrefab);
            if(!_localScale.ApproxEquals(to.localScale)) {to.localScale = _localScale;changed=true;}
            if(!_anchorMin.ApproxEquals(to.anchorMin)) {to.anchorMin = _anchorMin;changed=true;}
            if(!_anchorMax.ApproxEquals(to.anchorMax)) {to.anchorMax = _anchorMax;changed=true;}
            if(!_anchoredPosition.ApproxEquals(to.anchoredPosition)) {to.anchoredPosition = _anchoredPosition;changed=true;}
            if(!_sizeDelta.ApproxEquals(to.sizeDelta)) {to.sizeDelta = _sizeDelta;changed=true;}
            if(!_pivot.ApproxEquals(to.pivot)) {to.pivot = _pivot;changed=true;}
            return changed;
        }

        public static WidgetRectTransform Fill(float top = 0f, float bottom = 0f, float left = 0f, float right = 0f) 
            => Fill(new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one, top, bottom, left, right);

        public static WidgetRectTransform Fill(in Vector2 pivot, float top = 0f, float bottom = 0f, float left = 0f, float right = 0f) 
            => Fill(pivot, Vector2.zero, Vector2.one, top, bottom, left, right);

        public static WidgetRectTransform Fill(in Optional<Vector2> anchorMin, in Optional<Vector2> anchorMax, float top = 0f, float bottom = 0f, float left = 0f, float right = 0f) 
            => Fill(new Vector2(.5f, .5f),anchorMin, anchorMax, top, bottom, left, right);

        public static WidgetRectTransform Fill(TextAnchor from, float top = 0f, float bottom = 0f, float left = 0f, float right = 0f) 
            => Fill(CalcPivot(from), Vector2.zero, Vector2.one, top, bottom, left, right);

        public static WidgetRectTransform Fill(TextAnchor from, TextAnchor to, float top = 0f, float bottom = 0f, float left = 0f, float right = 0f)
        {
            var a = CalcPivot(from);
            var b = CalcPivot(to);
            var min = Vector2.Min(a, b);
            var max = Vector2.Max(a, b);
            return Fill(a, min, max, top, bottom, left, right);
        }

        public static WidgetRectTransform Fill(in Vector2 pivot, in Optional<Vector2> anchorMin, in Optional<Vector2> anchorMax, float top = 0f, float bottom = 0f, float left = 0f, float right = 0f)
        {
            return new WidgetRectTransform()
            {
                anchorMin = anchorMin,
                anchorMax = anchorMax,
                anchoredPosition = new Vector2((1-pivot.x)*left-(pivot.x)*right, 
                    (1-pivot.y)*bottom-(pivot.y)*top),
                sizeDelta = new Vector2(-(right+left), -(bottom+top)),
                pivot = pivot,
                localScale = Vector3.one,
            };
        }

        public static WidgetRectTransform From(TextAnchor from, in Optional<Vector2> pivot, in Optional<Vector2> size=default, in Optional<Vector2> offset=default)
        {
            var anchorMin = Vector2.zero;
            var anchorMax = Vector2.zero;
            
            anchorMin = anchorMax = CalcPivot(from);
            
            return new WidgetRectTransform()
            {
                anchorMin = anchorMin,
                anchorMax = anchorMax,
                anchoredPosition = offset,
                sizeDelta = size,
                pivot = pivot,
            };
        }
        
        public static WidgetRectTransform From(TextAnchor from, in Optional<Vector2> size=default, in Optional<Vector2> offset=default)
        {
            var anchorMin = Vector2.zero;
            var anchorMax = Vector2.zero;
            var pivot = Vector2.zero;
            
            pivot = anchorMin = anchorMax = CalcPivot(from);
            
            return new WidgetRectTransform()
            {
                anchorMin = anchorMin,
                anchorMax = anchorMax,
                anchoredPosition = offset,
                sizeDelta = size,
                pivot = pivot,
            };
        }

        static Vector2 CalcPivot(TextAnchor _from)
        {
            switch (_from)
            {
                case TextAnchor.UpperLeft:
                    return new Vector2(0, 1);

                case TextAnchor.UpperCenter:
                    return new Vector2(.5f, 1);

                case TextAnchor.UpperRight:
                    return new Vector2(1, 1);

                case TextAnchor.MiddleLeft:
                    return new Vector2(0, .5f);

                case TextAnchor.MiddleCenter:
                    return new Vector2(.5f, .5f);

                case TextAnchor.MiddleRight:
                    return new Vector2(1, .5f);

                case TextAnchor.LowerLeft:
                    return new Vector2(0, 0);

                case TextAnchor.LowerCenter:
                    return new Vector2(.5f, 0);

                case TextAnchor.LowerRight:
                    return new Vector2(1, 0);
                
                default:
                    return Vector2.zero;
            }
        }

        public bool Equals(WidgetRectTransform other)
        {
            return localScale.Equals(other.localScale) && anchorMin.Equals(other.anchorMin) && anchorMax.Equals(other.anchorMax) && anchoredPosition.Equals(other.anchoredPosition) && sizeDelta.Equals(other.sizeDelta) && pivot.Equals(other.pivot);
        }

        public override bool Equals(object obj)
        {
            return obj is WidgetRectTransform other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = localScale.GetHashCode();
                hashCode = (hashCode * 397) ^ anchorMin.GetHashCode();
                hashCode = (hashCode * 397) ^ anchorMax.GetHashCode();
                hashCode = (hashCode * 397) ^ anchoredPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ sizeDelta.GetHashCode();
                hashCode = (hashCode * 397) ^ pivot.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(WidgetRectTransform left, WidgetRectTransform right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WidgetRectTransform left, WidgetRectTransform right)
        {
            return !left.Equals(right);
        }
        
        public static WidgetRectTransform ScreenPoint(in Vector3 _position, in WidgetBuilder builder, Optional<Vector2> pivot=default, Optional<Vector2> size=default, Vector2 offset = default, Vector3 fallbackScreenPoint = default)
        {
            var position = ScreenToUIPosition(_position, builder.ParentWidget.Canvas, builder.ParentWidget.RectTransform, fallbackScreenPoint);
            var trueOffset = offset + new Vector2(position.x, position.y);
            return From(TextAnchor.MiddleCenter, pivot, size, trueOffset);
        }
        
        public static WidgetRectTransform ScreenPoint(in Vector3 _position, Canvas canvas, RectTransform parentTransform, Optional<Vector2> pivot=default, Optional<Vector2> size=default, Vector2 offset = default, Vector3 fallbackScreenPoint = default)
        {
            var  position = ScreenToUIPosition(_position, canvas, parentTransform, fallbackScreenPoint);
            var trueOffset = offset + new Vector2(position.x, position.y);
            return From(TextAnchor.MiddleCenter, pivot, size, trueOffset);
        }
        
        static Vector3 ScreenToUIPosition(in Vector3 position, Canvas canvas, RectTransform parentTransform, Vector3 fallbackScreenPoint)
        {
            var canvasTransform = (RectTransform)canvas.transform;
            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    return parentTransform.InverseTransformPoint(position);

                case RenderMode.ScreenSpaceCamera:
                    return parentTransform.InverseTransformPoint(canvas.worldCamera.ScreenToWorldPoint(position));

                case RenderMode.WorldSpace:
                    var plane = new Plane(-canvasTransform.forward, parentTransform.position);
                    var ray = canvas.worldCamera.ScreenPointToRay(position);
                    return plane.Raycast(ray, out var dist)
                        ? parentTransform.InverseTransformPoint(ray.GetPoint(dist))
                        : fallbackScreenPoint;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static WidgetRectTransform ScreenRect(in Rect _rect, Canvas canvas, RectTransform parentTransform, in WidgetRectOffset padding=default, in Vector3 fallbackScreenPoint=default)
        {
            var a = ScreenToUIPosition(_rect.min.XY_(), canvas, parentTransform, fallbackScreenPoint)+new Vector3(-padding.left, -padding.bottom);
            var b = ScreenToUIPosition(_rect.max.XY_(), canvas, parentTransform, fallbackScreenPoint)+new Vector3(padding.right, padding.top);
            var max = Vector3.Max(a, b);
            var min = Vector3.Min(a, b);
            var size = max - min;
            return WidgetRectTransform.From(TextAnchor.MiddleCenter, size.XY(), (a+size*0.5f).XY());
        }
    }
}