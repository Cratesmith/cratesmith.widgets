using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
    public struct SButton : IWidgetStateImage, IEquatable<SButton>
    {
        public Action<WButton>      onClick;
        public WidgetColorBlock     colors;
        public Optional<Navigation> navigation;
        public Optional<bool>       interactable;
        public Optional<Sprite> sprite { get; set; }
        public Optional<Color> color { get; set; }
        public Optional<Image.Type> imageType { get; set; }
        public Optional<bool> preserveAspect { get; set; }
        public Optional<bool> raycastTarget => true;
        public WidgetRectTransform rectTransform { get; set; }
        public WidgetLayoutElement layoutElement { get; set; }
        public WidgetContentSizeFitter contentSizeFitter { get; set; }
        public Optional<LogLevel> debugLogging { get; set; }
        public WidgetBasicLayout layoutGroup { get; set; }
        public Optional<bool> autoDisableLayoutGroup { get; set; }

        public bool Equals(SButton other)
        {
            return Equals(onClick, other.onClick) && colors.Equals(other.colors) && navigation.Equals(other.navigation) && interactable.Equals(other.interactable) && sprite.Equals(other.sprite) && color.Equals(other.color) && imageType.Equals(other.imageType) && preserveAspect.Equals(other.preserveAspect) && raycastTarget.Equals(other.raycastTarget) && rectTransform.Equals(other.rectTransform) && layoutElement.Equals(other.layoutElement) && contentSizeFitter.Equals(other.contentSizeFitter) && debugLogging.Equals(other.debugLogging) && layoutGroup.Equals(other.layoutGroup) && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup);
        }

        public override bool Equals(object obj)
        {
            return obj is SButton other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (onClick != null ? onClick.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ colors.GetHashCode();
                hashCode = (hashCode * 397) ^ navigation.GetHashCode();
                hashCode = (hashCode * 397) ^ interactable.GetHashCode();
                hashCode = (hashCode * 397) ^ sprite.GetHashCode();
                hashCode = (hashCode * 397) ^ color.GetHashCode();
                hashCode = (hashCode * 397) ^ imageType.GetHashCode();
                hashCode = (hashCode * 397) ^ preserveAspect.GetHashCode();
                hashCode = (hashCode * 397) ^ raycastTarget.GetHashCode();
                hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
                hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
                hashCode = (hashCode * 397) ^ debugLogging.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SButton left, SButton right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SButton left, SButton right)
        {
            return !left.Equals(right);
        }
    }

    public struct WidgetColorBlock : IEquatable<WidgetColorBlock>
    {
        public Optional<Color> normalColor;
        public Optional<Color> highlightedColor;
        public Optional<Color> pressedColor;
        public Optional<Color> selectedColor;
        public Optional<Color> disabledColor;
        public Optional<float> colorMultiplier;
        public Optional<float> fadeDuration;

        public ColorBlock GetValue(in ColorBlock _defaults, bool usesTypePrefab)
        {
            return new ColorBlock()
            {
                colorMultiplier = colorMultiplier.GetValue(_defaults.colorMultiplier,usesTypePrefab),
                disabledColor = disabledColor.GetValue(_defaults.disabledColor,usesTypePrefab),
                fadeDuration = fadeDuration.GetValue(_defaults.fadeDuration,usesTypePrefab),
                highlightedColor = highlightedColor.GetValue(_defaults.highlightedColor,usesTypePrefab),
                normalColor = normalColor.GetValue(_defaults.normalColor,usesTypePrefab),
                pressedColor = pressedColor.GetValue(_defaults.pressedColor,usesTypePrefab),
                selectedColor = selectedColor.GetValue(_defaults.selectedColor,usesTypePrefab),
            };
        }
        public bool Equals(WidgetColorBlock other)
        {
            return normalColor.Equals(other.normalColor) && highlightedColor.Equals(other.highlightedColor) && pressedColor.Equals(other.pressedColor) && selectedColor.Equals(other.selectedColor) && disabledColor.Equals(other.disabledColor) && colorMultiplier.Equals(other.colorMultiplier) && fadeDuration.Equals(other.fadeDuration);
        }
        public override bool Equals(object obj)
        {
            return obj is WidgetColorBlock other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = normalColor.GetHashCode();
                hashCode = (hashCode * 397) ^ highlightedColor.GetHashCode();
                hashCode = (hashCode * 397) ^ pressedColor.GetHashCode();
                hashCode = (hashCode * 397) ^ selectedColor.GetHashCode();
                hashCode = (hashCode * 397) ^ disabledColor.GetHashCode();
                hashCode = (hashCode * 397) ^ colorMultiplier.GetHashCode();
                hashCode = (hashCode * 397) ^ fadeDuration.GetHashCode();
                return hashCode;
            }
        }
        public static bool operator ==(WidgetColorBlock left, WidgetColorBlock right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(WidgetColorBlock left, WidgetColorBlock right)
        {
            return !left.Equals(right);
        }
    }
    
    public class WButton : WImage<SButton>, IWidgetHasEvent<EClicked>
    {
        [SerializeField,ReadOnlyField] protected Button m_Button;
        WidgetEventStorage<EClicked>                    m_Clicked;
        
        public Button Button => m_Button;
        
        
        protected override void OnDisable()
        {
            base.OnDisable();
            m_Clicked.Clear();
        }

        protected override void Awake()
        {
            if (!this.EnsureComponent(ref m_LayoutElement))
            {
                m_LayoutElement.minHeight = 32;
                m_LayoutElement.minWidth = 32;
            }

            if (!this.EnsureComponent(ref m_Image))
            {
                m_Image.color = new Color(1, 1, 1, 1f);
                m_Image.sprite = null;
                m_Image.type = Image.Type.Sliced;
            }
            
            base.Awake();
            
            this.EnsureComponent(ref m_Button);
            m_Button.onClick.AddListener(HandleOnClick);
            m_Button.image = m_Image;
        }

        protected override void OnRefresh(ref WidgetBuilder builder)
        {
            var prefab = this.GetPrefab();
            m_Button.colors = State.colors.GetValue(prefab.m_Button.colors, UsesTypePrefab);
            m_Button.navigation = State.navigation.GetValue(prefab.m_Button.navigation, UsesTypePrefab);
            m_Button.interactable = State.interactable.GetValue(prefab.m_Button.interactable, UsesTypePrefab);
            base.OnRefresh(ref builder);
        }

        void HandleOnClick()
        {
            State.onClick?.Invoke(this);
            this.SetEvent(EClicked.ThisFrame());
            if (OwnerWidget) OwnerWidget.SetDirty();
        }
        
        ref WidgetEventStorage<EClicked> IWidgetHasEvent<EClicked>.EventStorage => ref m_Clicked;
    }
}


