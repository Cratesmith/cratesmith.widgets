using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.cratesmith.widgets
{
    public struct STextButton : IWidgetStateLayoutGroup<WidgetBasicLayout>, IEquatable<STextButton>
    {
        public SPanel panelState;
        public SText textState;
        public SButton buttonState;

        public Optional<WText> textPrefab;
        public Optional<WPanel> panelPrefab;
        public Optional<WButton> buttonPrefab;
        public Action<WTextButton> onClick;

        public WidgetRectTransform rectTransform { get; set; }
        public WidgetLayoutElement layoutElement { get; set; }
        public WidgetContentSizeFitter contentSizeFitter { get; set; }
        public Optional<LogLevel> debugLogging { get; set; }
        public WidgetBasicLayout layoutGroup { get; set; }
        public Optional<bool> autoDisableLayoutGroup { get; set; }

        public bool Equals(STextButton other)
        {
            return panelState.Equals(other.panelState) 
                   && textState.Equals(other.textState) 
                   && buttonState.Equals(other.buttonState) 
                   && textPrefab.Equals(other.textPrefab) 
                   && panelPrefab.Equals(other.panelPrefab) 
                   && buttonPrefab.Equals(other.buttonPrefab) 
                   && Equals(onClick, other.onClick) 
                   && rectTransform.Equals(other.rectTransform) 
                   && layoutElement.Equals(other.layoutElement) 
                   && contentSizeFitter.Equals(other.contentSizeFitter) 
                   && layoutGroup.Equals(other.layoutGroup) 
                   && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup)
                   && debugLogging.Equals(other.debugLogging);
        }

        public override bool Equals(object obj)
        {
            return obj is STextButton other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = panelState.GetHashCode();
                hashCode = (hashCode * 397) ^ textState.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonState.GetHashCode();
                hashCode = (hashCode * 397) ^ textPrefab.GetHashCode();
                hashCode = (hashCode * 397) ^ panelPrefab.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonPrefab.GetHashCode();
                hashCode = (hashCode * 397) ^ (onClick != null ? onClick.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
                hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(STextButton left, STextButton right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(STextButton left, STextButton right)
        {
            return !left.Equals(right);
        }
    }
    
    public class WTextButton : WBasicLayout<STextButton>, IWidgetHasEvent<EClicked>
    {
        [SerializeField] WPanel      m_PanelPrefab;
        [SerializeField] WButton     m_ButtonPrefab;
        [SerializeField] WText       m_TextPrefab;
        Action<WButton>              m_ButtonOnClick;
        Action<WButton>              m_ButtonStateClick;
        
        WidgetEventStorage<EClicked> m_Clicked;
        ref WidgetEventStorage<EClicked> IWidgetHasEvent<EClicked>.EventStorage => ref m_Clicked;

        protected override void Awake()
        {
            m_ButtonOnClick = src =>
            {
                m_ButtonStateClick?.Invoke(src);
                State.onClick?.Invoke(this);
                m_Clicked.Set(EClicked.ThisFrame());
                if (Is.Spawned(OwnerWidget)) OwnerWidget.SetDirty();
            };
            base.Awake();
        }

        public string Text => State.textState.text;

        protected override void OnRefresh(ref WidgetBuilder builder)
        {
            using (var panel = builder.Widget<WPanel>(GetValue(State.panelPrefab, m_PanelPrefab))
                .State(State.panelState).BeginChildren())
            {
                var buttonState = State.buttonState;
                m_ButtonStateClick = buttonState.onClick;
                buttonState.onClick = m_ButtonOnClick;

                using var button = panel.Widget<WButton>(GetValue(State.buttonPrefab, m_ButtonPrefab))
                    .State(buttonState)
                    .BeginChildren();
                
                button.Widget(GetValue(State.textPrefab, m_TextPrefab))
                    .State(State.textState);
            }
            
            base.OnRefresh(ref builder);
        }

        public override string ToString()
        {
            return $"{base.ToString()}:{State.textState.text.value}";
        }
    }
}