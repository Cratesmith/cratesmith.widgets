using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
	public struct SGridLayout : IWidgetStateLayoutGroup<WidgetGridLayout>, IEquatable<SGridLayout>
    {
        public Optional<bool> autoDisableLayoutGroup { get; set; }
        public WidgetRectTransform rectTransform { get; set; }
        public WidgetLayoutElement layoutElement { get; set; }
        public WidgetContentSizeFitter contentSizeFitter { get; set; }
        public Optional<LogLevel> debugLogging { get; set; }
        public WidgetGridLayout layoutGroup { get; set; }

        public bool Equals(SGridLayout other)
        {
	        return autoDisableLayoutGroup.Equals(other.autoDisableLayoutGroup) && rectTransform.Equals(other.rectTransform) && layoutElement.Equals(other.layoutElement) && contentSizeFitter.Equals(other.contentSizeFitter) && debugLogging.Equals(other.debugLogging) && layoutGroup.Equals(other.layoutGroup);
        }
        public override bool Equals(object obj)
        {
	        return obj is SGridLayout other && Equals(other);
        }
        public override int GetHashCode()
        {
	        unchecked
	        {
		        var hashCode = autoDisableLayoutGroup.GetHashCode();
		        hashCode = (hashCode * 397) ^ rectTransform.GetHashCode();
		        hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
		        hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
		        hashCode = (hashCode * 397) ^ debugLogging.GetHashCode();
		        hashCode = (hashCode * 397) ^ layoutGroup.GetHashCode();
		        return hashCode;
	        }
        }
        public static bool operator ==(SGridLayout left, SGridLayout right)
        {
	        return left.Equals(right);
        }
        public static bool operator !=(SGridLayout left, SGridLayout right)
        {
	        return !left.Equals(right);
        }
    }
	
	public class WGridLayout : WGridLayout<SGridLayout>
	{
	}
	
	public class WGridLayout<TState>
		: WidgetBehaviour<TState>
		where TState : struct, IWidgetStateLayoutGroup<WidgetGridLayout> //, IEquatable<TState>
	{
		[SerializeField]                          bool            m_AutoDisableLayoutGroup = true;
		[ReadOnlyField, SerializeField] protected GridLayoutGroup m_GridLayoutGroup;

		protected override void Awake()
		{
			this.EnsureComponent(ref m_GridLayoutGroup);

			base.Awake();
		}

		protected override void OnRefresh(ref WidgetBuilder builder)
		{
			// if (State.layoutGroup.Apply(m_GridLayoutGroup, this.GetPrefab().m_GridLayoutGroup, UsesTypePrefab) || !HasRefreshed)
			if (!IsSelfOwned)
			{
				State.layoutGroup.Apply(m_GridLayoutGroup, this.GetPrefab().m_GridLayoutGroup, UsesTypePrefab);
			}
			LayoutRebuilder.MarkLayoutForRebuild(RectTransform); // do this beforehand so LayoutGroup.DelayedSetDirty isn't called.

		}

		protected override void OnPostRefresh()
		{
			var prefab = this.GetPrefab();
			if (State.autoDisableLayoutGroup.GetValue(prefab.m_AutoDisableLayoutGroup, UsesTypePrefab))
				m_GridLayoutGroup.enabled = CalcNeedsLayoutGroup();
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
