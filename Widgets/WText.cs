using System;
using com.cratesmith.widgets;
using TMPro;
using UnityEngine;

[Serializable]
public struct SText : IWidgetStateLayoutGroup<WidgetBasicLayout>, IEquatable<SText>
{
    public Optional<string> text;
    public Optional<FontStyles> fontStyle;
    public Optional<float> alpha;
    public Optional<Color> color;
    public Optional<TMP_FontAsset> font;
    public Optional<float> fontSize;
    public Optional<bool> enableAutoSizing;
    public Optional<float> fontSizeMin;
    public Optional<float> fontSizeMax;
    public Optional<Vector4> margin;
    public Optional<float> characterSpacing;
    public Optional<float> lineSpacing;
    public Optional<VerticalAlignmentOptions> verticalAlignment;
    public Optional<HorizontalAlignmentOptions> horizontalAlignment;
    public Optional<TextOverflowModes> overflowMode;
    public Optional<bool> raycastTarget;
    public WidgetRectTransform rectTransform;
    WidgetRectTransform IWidgetState.rectTransform => this.rectTransform;

    public WidgetLayoutElement layoutElement;
    WidgetLayoutElement IWidgetState.layoutElement => this.layoutElement;

    public WidgetContentSizeFitter contentSizeFitter;
    WidgetContentSizeFitter IWidgetState.contentSizeFitter => this.contentSizeFitter;
    public Optional<LogLevel> debugLogging { get; set; }

    public WidgetBasicLayout layoutGroup;
    WidgetBasicLayout IWidgetStateLayoutGroup<WidgetBasicLayout>.layoutGroup => this.layoutGroup;
    public Optional<bool> autoDisableLayoutGroup { get; set; }

    public bool Equals(SText other)
    {
        return text.Equals(other.text) 
               && fontStyle.Equals(other.fontStyle) 
               && alpha.Equals(other.alpha) 
               && color.Equals(other.color) 
               && font.Equals(other.font) 
               && fontSize.Equals(other.fontSize) 
               && enableAutoSizing.Equals(other.enableAutoSizing) 
               && fontSizeMin.Equals(other.fontSizeMin) 
               && fontSizeMax.Equals(other.fontSizeMax) 
               && margin.Equals(other.margin) 
               && characterSpacing.Equals(other.characterSpacing) 
               && lineSpacing.Equals(other.lineSpacing) 
               && verticalAlignment.Equals(other.verticalAlignment) 
               && horizontalAlignment.Equals(other.horizontalAlignment) 
               && overflowMode.Equals(other.overflowMode) 
               && raycastTarget.Equals(other.raycastTarget) 
               && rectTransform.Equals(other.rectTransform) 
               && layoutElement.Equals(other.layoutElement) 
               && contentSizeFitter.Equals(other.contentSizeFitter) 
               && layoutGroup.Equals(other.layoutGroup) 
               && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup)
               && debugLogging.Equals(other.debugLogging);
    }

    public override bool Equals(object obj)
    {
        return obj is SText other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = text.GetHashCode();
            hashCode = (hashCode * 397) ^ fontStyle.GetHashCode();
            hashCode = (hashCode * 397) ^ alpha.GetHashCode();
            hashCode = (hashCode * 397) ^ color.GetHashCode();
            hashCode = (hashCode * 397) ^ font.GetHashCode();
            hashCode = (hashCode * 397) ^ fontSize.GetHashCode();
            hashCode = (hashCode * 397) ^ enableAutoSizing.GetHashCode();
            hashCode = (hashCode * 397) ^ fontSizeMin.GetHashCode();
            hashCode = (hashCode * 397) ^ fontSizeMax.GetHashCode();
            hashCode = (hashCode * 397) ^ margin.GetHashCode();
            hashCode = (hashCode * 397) ^ characterSpacing.GetHashCode();
            hashCode = (hashCode * 397) ^ lineSpacing.GetHashCode();
            hashCode = (hashCode * 397) ^ verticalAlignment.GetHashCode();
            hashCode = (hashCode * 397) ^ horizontalAlignment.GetHashCode();
            hashCode = (hashCode * 397) ^ overflowMode.GetHashCode();
            hashCode = (hashCode * 397) ^ raycastTarget.GetHashCode();
            hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
            hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
            hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
            hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
            hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(SText left, SText right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SText left, SText right)
    {
        return !left.Equals(right);
    }
} 

public class WText : WBasicLayout<SText>
{
    [SerializeField, ReadOnlyField] protected TextMeshProUGUI m_Text;
    
    protected override void Awake()
    {
        if (!this.EnsureComponent(ref m_Text))
        {
            m_Text.raycastTarget = false;
            m_Text.color = Color.black;
            m_Text.fontSize = 25;
        }
        base.Awake();
    }

    protected override void OnRefresh(ref WidgetBuilder builder)
    {
        var prefab = this.GetPrefab();
        m_Text.text = GetValue(State.text, prefab.m_Text.text);
        m_Text.raycastTarget = GetValue(State.raycastTarget, prefab.m_Text.raycastTarget);
        m_Text.verticalAlignment = GetValue(State.verticalAlignment, prefab.m_Text.verticalAlignment);
        m_Text.horizontalAlignment = GetValue(State.horizontalAlignment, prefab.m_Text.horizontalAlignment);
        m_Text.overflowMode = GetValue(State.overflowMode, prefab.m_Text.overflowMode);
        m_Text.fontStyle = GetValue(State.fontStyle, prefab.m_Text.fontStyle);
        m_Text.alpha = GetValue(State.alpha, prefab.m_Text.alpha);
        m_Text.color = GetValue(State.color, prefab.m_Text.color);
        m_Text.font = GetValue(State.font, prefab.m_Text.font);
        m_Text.fontSize = GetValue(State.fontSize, prefab.m_Text.fontSize);
        m_Text.enableAutoSizing = GetValue(State.enableAutoSizing, prefab.m_Text.enableAutoSizing);
        m_Text.fontSizeMin = GetValue(State.fontSizeMin, prefab.m_Text.fontSizeMin);
        m_Text.fontSizeMax = GetValue(State.fontSizeMax, prefab.m_Text.fontSizeMax);
        m_Text.margin = GetValue(State.margin, prefab.m_Text.margin);
        m_Text.characterSpacing = GetValue(State.characterSpacing, prefab.m_Text.characterSpacing);
        m_Text.lineSpacing = GetValue(State.lineSpacing, prefab.m_Text.lineSpacing);
        base.OnRefresh(ref builder);
    }
}