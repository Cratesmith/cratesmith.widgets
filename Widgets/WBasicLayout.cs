using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
    public struct SBasicLayout : IWidgetStateLayoutGroup<WidgetBasicLayout>, IEquatable<SBasicLayout>
    {
        public Optional<bool> autoDisableLayoutGroup { get; set; }
        public WidgetRectTransform rectTransform { get; set; }
        public WidgetLayoutElement layoutElement { get; set; }
        public WidgetContentSizeFitter contentSizeFitter { get; set; }
        public Optional<LogLevel> debugLogging { get; set; }
        public WidgetBasicLayout layoutGroup { get; set; }

        public static SBasicLayout Vertical(Optional<TextAnchor> _childAlignment=default)
        {
            return new SBasicLayout()
            {
                layoutGroup = WidgetBasicLayout.Vertical(_childAlignment)
            };
        }
        
        public static SBasicLayout Horizontal(Optional<TextAnchor> _childAlignment=default)
        {
            return new SBasicLayout()
            {
                layoutGroup = WidgetBasicLayout.Horizontal(_childAlignment)
            };
        }
        
        public bool Equals(SBasicLayout other)
        {
            return autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup) 
                   && rectTransform.Equals(other.rectTransform) 
                   && layoutElement.Equals(other.layoutElement) 
                   && contentSizeFitter.Equals(other.contentSizeFitter) 
                   && layoutGroup.Equals(other.layoutGroup) 
                   && debugLogging.Equals(other.debugLogging);
        }

        public override bool Equals(object obj)
        {
            return obj is SBasicLayout other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = autoDisableLayoutGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
                hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SBasicLayout left, SBasicLayout right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SBasicLayout left, SBasicLayout right)
        {
            return !left.Equals(right);
        }
    }
    
    public class WBasicLayout : WBasicLayout<SBasicLayout>
    {
    }
    
    public class WBasicLayout<TState> 
        : WidgetBehaviour<TState> 
        where TState : struct, IWidgetStateLayoutGroup<WidgetBasicLayout>//, IEquatable<TState>
    {
        [SerializeField]                          bool             m_AutoDisableLayoutGroup = true;
        [ReadOnlyField, SerializeField] protected BasicLayoutGroup m_BasicLayoutGroup;
        
        protected override void Awake()
        {
            if (!this.EnsureComponent(ref m_BasicLayoutGroup))
            {
                m_BasicLayoutGroup.isVertical = true;
                m_BasicLayoutGroup.childForceExpandWidth = false;
                m_BasicLayoutGroup.childForceExpandHeight = false;
                m_BasicLayoutGroup.childControlWidth = true;
                m_BasicLayoutGroup.childControlHeight = true;
            }
            m_BasicLayoutGroup.autoMarkForRebuild = false;
            
            base.Awake();
        }
   
        protected override void OnRefresh(ref WidgetBuilder builder)
        {
            if (!IsSelfOwned)
            {
                State.layoutGroup.Apply(m_BasicLayoutGroup, this.GetPrefab().m_BasicLayoutGroup, UsesTypePrefab);
            }
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform); // do this beforehand so LayoutGroup.DelayedSetDirty isn't called.
            // if (State.layoutGroup.Apply(m_BasicLayoutGroup, this.GetPrefab().m_BasicLayoutGroup, UsesTypePrefab)
            //          || !HasRefreshed) 
            //      LayoutRebuilder.MarkLayoutForRebuild(RectTransform); // do this beforehand so LayoutGroup.DelayedSetDirty isn't called.
        }

        protected override void OnPostRefresh()
        {
            var prefab = this.GetPrefab();
            if(State.autoDisableLayoutGroup.GetValue(prefab.m_AutoDisableLayoutGroup,UsesTypePrefab))
                m_BasicLayoutGroup.enabled = CalcNeedsLayoutGroup();
        }

        bool CalcNeedsLayoutGroup()
        {
            bool needsLayoutGroup = !m_LayoutElement.ignoreLayout;
            if (!needsLayoutGroup)
            {
                foreach (var child in Children)
                {
                    if (!child.widget.LayoutElement.ignoreLayout)
                    {
                        needsLayoutGroup = true;
                        break;
                    }
                }
            }
            return needsLayoutGroup;
        }
    }

}
