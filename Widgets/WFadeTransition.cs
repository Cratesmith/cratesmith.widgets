using System;
using UnityEngine;

namespace com.cratesmith.widgets
{
	public struct SFadeTransition
		: IWidgetStateLayoutGroup<WidgetBasicLayout>
		, IEquatable<SFadeTransition>
	{
		public Optional<float> fadeInTime;
		public Optional<float> fadeOutTime;
		public WidgetRectTransform rectTransform { get; set; }
		public WidgetLayoutElement layoutElement { get; set; }
		public WidgetContentSizeFitter contentSizeFitter { get; set; }
		public Optional<LogLevel> debugLogging { get; set; }
		public WidgetBasicLayout layoutGroup { get; set; }
		public Optional<bool> autoDisableLayoutGroup { get; set; }
		public bool Equals(SFadeTransition other)
		{
			return fadeInTime.Equals(other.fadeInTime) 
			       && fadeOutTime.Equals(other.fadeOutTime) 
			       && rectTransform.Equals(other.rectTransform) 
			       && layoutElement.Equals(other.layoutElement) 
			       && contentSizeFitter.Equals(other.contentSizeFitter) 
			       && layoutGroup.Equals(other.layoutGroup) 
			       && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup) 
			       && debugLogging.Equals(other.debugLogging);
		}
		public override bool Equals(object obj)
		{
			return obj is SFadeTransition other && Equals(other);
		}
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = fadeInTime.GetHashCode();
				hashCode = (hashCode * 397) ^ fadeOutTime.GetHashCode();
				hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
				hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
				hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
				hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
				hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
				return hashCode;
			}
		}
		public static bool operator ==(SFadeTransition left, SFadeTransition right) => left.Equals(right);
		public static bool operator !=(SFadeTransition left, SFadeTransition right) => !left.Equals(right);
	}

	public class WFadeTransition : WBasicLayout<SFadeTransition>
	{
		[ReadOnlyField, SerializeField] CanvasGroup m_CanvasGroup;
		[SerializeField]                float       m_FadeInTime = .5f;
		[SerializeField]                float       m_FadeOutTime = .5f;
		bool                                        m_FadingOut;

		protected override void Awake()
		{
			this.EnsureComponent(ref m_CanvasGroup); 
			base.Awake();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			m_FadingOut = false;
			if(Application.isPlaying)
				m_CanvasGroup.alpha = 0;
		}

		protected override bool TryToDespawn()
		{
			m_FadingOut = true;
			return m_CanvasGroup.alpha <= 0f;
		}

		void Update()
		{
			if (Application.isPlaying)
			{
				if (m_FadingOut)
				{
					var fadeTime = GetValue(State.fadeOutTime,m_FadeOutTime);
					m_CanvasGroup.alpha = Mathf.Clamp01(fadeTime>0 
							            ? m_CanvasGroup.alpha - Time.deltaTime/fadeTime
							            : 0f);
				} else
				{
					var fadeTime = GetValue(State.fadeInTime, m_FadeInTime);
					m_CanvasGroup.alpha = Mathf.Clamp01(fadeTime>0 
	                                    ? m_CanvasGroup.alpha + Time.deltaTime/fadeTime
	                                    : 1f);	
				}
			}
		}
		
		public override string ToString()
		{
			if (Children.Count == 0) return base.ToString();
			return $"{base.ToString()} -> {Children[Children.Count-1]}";
		}
	}
}
