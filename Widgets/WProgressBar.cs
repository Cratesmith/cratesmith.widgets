using System;
using com.cratesmith.widgets;
using UnityEngine;
using UnityEngine.UI;

public struct SProgressBar : IWidgetStateLayoutGroup<WidgetBasicLayout>, IEquatable<SProgressBar>
{
    public Optional<Sprite> barSprite;
    public Optional<Color> barColor;
    public Optional<WidgetRectOffset> barPadding;
    public Optional<Sprite> backgroundSprite;
    public Optional<Color> backgroundColor;
    public Optional<WidgetRectOffset> backgroundPadding;
    public Optional<float> progress;

    public WidgetRectTransform rectTransform { get; set;}
    public WidgetLayoutElement layoutElement { get; set; }
    public WidgetContentSizeFitter contentSizeFitter { get;set; }
    public Optional<LogLevel> debugLogging { get; set; }
    public WidgetBasicLayout layoutGroup { get;set; }
    public Optional<bool> autoDisableLayoutGroup { get; set; }

    public bool Equals(SProgressBar other)
    {
        return barSprite.Equals(other.barSprite) 
               && barColor.Equals(other.barColor) 
               && barPadding.Equals(other.barPadding) 
               && backgroundSprite.Equals(other.backgroundSprite) 
               && backgroundColor.Equals(other.backgroundColor) 
               && backgroundPadding.Equals(other.backgroundPadding) 
               && progress.Equals(other.progress) 
               && rectTransform.Equals(other.rectTransform) 
               && layoutElement.Equals(other.layoutElement) 
               && contentSizeFitter.Equals(other.contentSizeFitter) 
               && layoutGroup.Equals(other.layoutGroup) 
               && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup)
               && debugLogging.Equals(other.debugLogging);
    }

    public override bool Equals(object obj)
    {
        return obj is SProgressBar other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = barSprite.GetHashCode();
            hashCode = (hashCode * 397) ^ barColor.GetHashCode();
            hashCode = (hashCode * 397) ^ barPadding.GetHashCode();
            hashCode = (hashCode * 397) ^ backgroundSprite.GetHashCode();
            hashCode = (hashCode * 397) ^ backgroundColor.GetHashCode();
            hashCode = (hashCode * 397) ^ backgroundPadding.GetHashCode();
            hashCode = (hashCode * 397) ^ progress.GetHashCode();
            hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
            hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
            hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
            hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
            hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(SProgressBar left, SProgressBar right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SProgressBar left, SProgressBar right)
    {
        return !left.Equals(right);
    }
}

public class WProgressBar : WBasicLayout<SProgressBar>
{
    [SerializeField] Sprite m_BackgroundSprite;
    [SerializeField] Color m_BackgroundColor = Color.black;
    [SerializeField] Sprite m_BarSprite;
    [SerializeField] Color m_BarColor = Color.white;
    [SerializeField] WidgetRectOffset m_BarPadding = new WidgetRectOffset(2,2,2,2);
    [SerializeField] WidgetRectOffset m_BackgroundPadding;
    [SerializeField] float m_Progress = 1;
    
    protected override void Awake()
    {
        if (!this.EnsureComponent(ref m_LayoutElement))
        {
            m_LayoutElement.preferredHeight = 16;
            m_LayoutElement.flexibleWidth = 1;
        }
        
        base.Awake();
    }

    protected override void OnRefresh(ref WidgetBuilder builder)
    {
        var prefab = this.GetPrefab();
        var anchorMin = Vector2.zero;
        var anchorMax = new Vector2(Mathf.Clamp01(GetValue(State.progress, prefab.m_Progress)), 1);
        var barPadding = GetValue(State.barPadding, prefab.m_BarPadding);
        var bgPadding = GetValue(State.backgroundPadding, prefab.m_BackgroundPadding);
        
        // bar
        builder.Widget<WImage>().State(new SImage()
        {
            layoutElement = WidgetLayoutElement.IgnoreLayout(),
            rectTransform = WidgetRectTransform.Fill(bgPadding.top, bgPadding.bottom, bgPadding.left, bgPadding.right),
            sprite = GetValue(State.backgroundSprite, prefab.m_BackgroundSprite),
            color = GetValue(State.backgroundColor, prefab.m_BackgroundColor),
            imageType = Image.Type.Sliced
        });

        using (var padding = builder.Widget<WBasicLayout>().State(new SBasicLayout()
        {
            layoutElement = WidgetLayoutElement.IgnoreLayout(),
            rectTransform = WidgetRectTransform.Fill(barPadding.top, barPadding.bottom, barPadding.left, barPadding.right),
        }).BeginChildren())
        {
            padding.Widget<WImage>().State(new SImage()
            {
                layoutElement = WidgetLayoutElement.IgnoreLayout(),
                rectTransform = WidgetRectTransform.Fill(anchorMin, anchorMax),
                sprite = GetValue(State.barSprite, prefab.m_BarSprite),
                color = GetValue(State.barColor, prefab.m_BarColor),
                imageType = Image.Type.Sliced,
            });
        }
        
        base.OnRefresh(ref builder);
    }
}