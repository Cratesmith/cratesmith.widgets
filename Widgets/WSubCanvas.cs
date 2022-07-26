using System;
using UnityEngine;

namespace com.cratesmith.widgets
{
	public struct SSubCanvas : IWidgetStateLayoutGroup<WidgetBasicLayout>, IEquatable<SSubCanvas>
	{
		public Optional<bool> overrideSorting;
		public Optional<int>  sortOrder;
		public Optional<bool> overridePixelPerfect;
		public Optional<bool> pixelPerfect;
		public Optional<AdditionalCanvasShaderChannels> additionalShaderChannels;
		
		public WidgetRectTransform rectTransform { get; set; }
		public WidgetLayoutElement layoutElement { get; set; }
		public WidgetContentSizeFitter contentSizeFitter { get; set; }
		public Optional<LogLevel> debugLogging { get; set; }
		public WidgetBasicLayout layoutGroup { get; set; }
		public Optional<bool> autoDisableLayoutGroup { get; set; }
		public bool Equals(SSubCanvas other)
		{
			return overrideSorting.Equals(other.overrideSorting) && sortOrder.Equals(other.sortOrder) && overridePixelPerfect.Equals(other.overridePixelPerfect) && pixelPerfect.Equals(other.pixelPerfect) && additionalShaderChannels.Equals(other.additionalShaderChannels) && rectTransform.Equals(other.rectTransform) && layoutElement.Equals(other.layoutElement) && contentSizeFitter.Equals(other.contentSizeFitter) && debugLogging.Equals(other.debugLogging) && layoutGroup.Equals(other.layoutGroup) && autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup);
		}
		public override bool Equals(object obj)
		{
			return obj is SSubCanvas other && Equals(other);
		}
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = overrideSorting.GetHashCode();
				hashCode = (hashCode * 397) ^ sortOrder.GetHashCode();
				hashCode = (hashCode * 397) ^ overridePixelPerfect.GetHashCode();
				hashCode = (hashCode * 397) ^ pixelPerfect.GetHashCode();
				hashCode = (hashCode * 397) ^ additionalShaderChannels.GetHashCode();
				hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
				hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
				hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
				hashCode = (hashCode * 397) ^ debugLogging.GetHashCode();
				hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
				hashCode = (hashCode * 397) ^ autoDisableLayoutGroup.GetHashCode();
				return hashCode;
			}
		}
		public static bool operator ==(SSubCanvas left, SSubCanvas right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(SSubCanvas left, SSubCanvas right)
		{
			return !left.Equals(right);
		}
	}
	
	[RequireComponent(typeof(Canvas))]
	public class WSubCanvas : WBasicLayout<SSubCanvas>
	{
		private Canvas m_Canvas;
		
		protected override void Awake()
		{
			this.EnsureComponent(ref m_Canvas);
			base.Awake();
		}

		protected override void OnRefresh(ref WidgetBuilder builder)
		{
			base.OnRefresh(ref builder);

			var prefab = this.GetPrefab();
			var _overrideSorting = GetValue(State.overrideSorting, prefab.m_Canvas.overrideSorting);
			if (_overrideSorting != m_Canvas.overrideSorting)
			{
				m_Canvas.overrideSorting = _overrideSorting;
			}
			
			var _sortOrder = GetValue(State.sortOrder, prefab.m_Canvas.sortingOrder);
			if (_sortOrder != m_Canvas.sortingOrder)
			{
				m_Canvas.sortingOrder = _sortOrder;
			}

			var _overridePixelPerfect = GetValue(State.overridePixelPerfect, prefab.m_Canvas.overridePixelPerfect);
			if (_overridePixelPerfect != m_Canvas.overridePixelPerfect)
			{
				m_Canvas.overridePixelPerfect = _overridePixelPerfect;
			}
			
			var _pixelPerfect = GetValue(State.pixelPerfect, prefab.m_Canvas.pixelPerfect);
			if (_pixelPerfect != m_Canvas.pixelPerfect)
			{
				m_Canvas.pixelPerfect = _pixelPerfect;
			}

			var _additionalShaderChannels = GetValue(State.additionalShaderChannels, prefab.m_Canvas.additionalShaderChannels);
			if (_additionalShaderChannels != m_Canvas.additionalShaderChannels)
			{
				m_Canvas.additionalShaderChannels = _additionalShaderChannels;
			}
		}
	}
}
