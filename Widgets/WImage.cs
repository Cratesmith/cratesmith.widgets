using System;
using System.Collections;
using System.Collections.Generic;
using com.cratesmith.widgets;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
    public struct SImage : IWidgetStateImage, IEquatable<SImage>
    {
        public Optional<Sprite> sprite { get; set; }
        public Optional<Color> color { get; set; }
        public Optional<Image.Type> imageType { get; set; }
        public Optional<bool> preserveAspect { get; set; }
        public WidgetRectTransform rectTransform { get; set; }
        public WidgetLayoutElement layoutElement { get; set; }
        public WidgetBasicLayout layoutGroup { get; set; }
        public Optional<bool> autoDisableLayoutGroup { get; set; }
        public WidgetContentSizeFitter contentSizeFitter { get; set; }
        public Optional<LogLevel> debugLogging { get; set; }

        public bool Equals(SImage other)
        {
            return sprite.Equals(other.sprite) 
                   && color.Equals(other.color) 
                   && imageType.Equals(other.imageType) 
                   && preserveAspect.Equals(other.preserveAspect) 
                   && rectTransform.Equals(other.rectTransform) && layoutElement.Equals(other.layoutElement) && layoutGroup.Equals(other.layoutGroup) 
                   && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup) 
                   && contentSizeFitter.Equals(other.contentSizeFitter)
                   && debugLogging.Equals(other.debugLogging);
        }

        public override bool Equals(object obj)
        {
            return obj is SImage other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = sprite.GetHashCode();
                hashCode = (hashCode * 397) ^ color.GetHashCode();
                hashCode = (hashCode * 397) ^ imageType.GetHashCode();
                hashCode = (hashCode * 397) ^ preserveAspect.GetHashCode();
                hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SImage left, SImage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SImage left, SImage right)
        {
            return !left.Equals(right);
        }
    }

    public interface IWidgetStateImage : IWidgetStateLayoutGroup<WidgetBasicLayout>
    {
        public Optional<Sprite> sprite { get; }
        public Optional<Color> color { get; }
        public Optional<Image.Type> imageType { get; }
        public Optional<bool> preserveAspect { get; }
    }

    public class WImage : WImage<SImage>
    {
    }
    
    public class WImage<TState> 
        : WBasicLayout<TState> 
        where TState: struct, IWidgetStateImage//, IEquatable<TState>
    {
        [SerializeField, ReadOnlyField] protected Image m_Image;
        public Image Image => m_Image;
        
        protected override void Awake()
        {
            base.Awake();
            if (!this.EnsureComponent(ref m_Image))
            {
                m_Image.color = new Color(1, 1, 1, 1f);
                m_Image.sprite = null;
                m_Image.type = Image.Type.Simple;
            }
        }

        protected override void OnRefresh(ref WidgetBuilder builder)
        {
            var prefab = this.GetPrefab();
            m_Image.sprite = GetValue(State.sprite, prefab.m_Image.sprite);
            m_Image.color = GetValue(State.color, prefab.m_Image.color);
            m_Image.type = GetValue(State.imageType, prefab.m_Image.type);
            m_Image.preserveAspect = State.preserveAspect.GetValue(prefab.m_Image.preserveAspect, UsesTypePrefab);
            base.OnRefresh(ref builder);
        }
    }
}

