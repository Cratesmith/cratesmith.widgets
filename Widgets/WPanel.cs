using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
    public struct SPanel : IWidgetStateLayoutGroup<WidgetBasicLayout>, IEquatable<SPanel>
    {
        public Optional<Sprite> backgroundSprite;
        public Optional<Color>  backgroundColor;
        public Optional<WidgetRectOffset> backgroundOffset;

        public WidgetRectTransform rectTransform { get; set; }
        public WidgetLayoutElement layoutElement { get; set; }
        public WidgetContentSizeFitter contentSizeFitter { get; set; }
        public Optional<LogLevel> debugLogging { get; set; }
        public WidgetBasicLayout layoutGroup { get; set; }
        public Optional<bool> autoDisableLayoutGroup { get; set; }

        public bool Equals(SPanel other)
        {
            return backgroundSprite.Equals(other.backgroundSprite) 
                   && backgroundColor.Equals(other.backgroundColor) 
                   && backgroundOffset.Equals(other.backgroundOffset) 
                   && rectTransform.Equals(other.rectTransform) 
                   && layoutElement.Equals(other.layoutElement) 
                   && contentSizeFitter.Equals(other.contentSizeFitter) 
                   && layoutGroup.Equals(other.layoutGroup) 
                   && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup)
                   && debugLogging.Equals(other.debugLogging);
        }

        public override bool Equals(object obj)
        {
            return obj is SPanel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = backgroundSprite.GetHashCode();
                hashCode = (hashCode * 397) ^ backgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ backgroundOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
                hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SPanel left, SPanel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SPanel left, SPanel right)
        {
            return !left.Equals(right);
        }
    }
    
    public class WPanel : WBasicLayout<SPanel>
    {
        [SerializeField] Sprite m_BackgroundSprite;
        [SerializeField] Color  m_BackgroundColor = new Color(0,0,0,.5f);
        [SerializeField] WidgetRectOffset m_BackgroundOffset = new WidgetRectOffset(-2, -2, -2, -2);

        protected override void OnRefresh(ref WidgetBuilder builder)
        {
            var prefab = this.GetPrefab();
            var backgroundOffset = State.backgroundOffset.GetValue(prefab.m_BackgroundOffset, UsesTypePrefab);
            builder.Widget<WImage>().State(new SImage()
            {
                layoutElement = WidgetLayoutElement.IgnoreLayout(),
                rectTransform = WidgetRectTransform.Fill(backgroundOffset.top,backgroundOffset.bottom, backgroundOffset.left, backgroundOffset.right),
                sprite = GetValue(State.backgroundSprite, prefab.m_BackgroundSprite),
                color = GetValue(State.backgroundColor, prefab.m_BackgroundColor),
                imageType = Image.Type.Sliced,
            });

            base.OnRefresh(ref builder);
        }
    }
}