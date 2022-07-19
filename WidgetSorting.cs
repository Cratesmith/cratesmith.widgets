using System;

namespace com.cratesmith.widgets
{
	public struct WidgetSorting : IEquatable<WidgetSorting>, IComparable<WidgetSorting>
	{
		public const int STATIC_CHILDREN   = 1000;
		public const int INTERNAL_CHILDREN = 2000;
		public const int OWNER_CHILDREN    = 3000;
		
		public readonly int group;
		public readonly int index;
		
		public WidgetSorting(int group, int index)
		{
			this.group = group;
			this.index = index;
		}

		public bool Equals(WidgetSorting other)
		{
			return group == other.group && index == other.index;
		}
		
		public override bool Equals(object obj)
		{
			return obj is WidgetSorting other && Equals(other);
		}
		
		public override int GetHashCode()
		{
			unchecked
			{
				return (group * 397) ^ index;
			}
		}
		
		public static bool operator ==(WidgetSorting left, WidgetSorting right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(WidgetSorting left, WidgetSorting right)
		{
			return !left.Equals(right);
		}
		public int CompareTo(WidgetSorting other)
		{
			int groupComparison = group.CompareTo(other.group);

			if (groupComparison != 0)
			{
				return groupComparison;
			}

			return index.CompareTo(other.index);
		}
		
		public override string ToString()
		{
			return $"{nameof(group)}: {group}, {nameof(index)}: {index}";
		}	
	}
}
