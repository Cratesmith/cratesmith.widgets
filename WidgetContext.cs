using System;

namespace com.cratesmith.widgets
{
	public struct WidgetContext : IEquatable<WidgetContext>
	{
		public readonly int    id;
		public readonly object reference;
		public readonly Type   type;
		public readonly bool   isUnique;
		
		public WidgetContext(int _id, Type _type, bool _isUnique, object _reference)
		{
			id = _id;
			type = _type;
			isUnique = _isUnique;
			reference = _reference;
		}
		
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = id;
				hashCode = (hashCode * 397) ^ (type != null ? type.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ isUnique.GetHashCode();
				hashCode = (hashCode * 397) ^ (type != null ? reference.GetHashCode() : 0);
				return hashCode;
			}
		}
		public bool Equals(WidgetContext other)
		{
			return id == other.id 
			       && Equals(type, other.type) 
			       && isUnique == other.isUnique
			       && ReferenceEquals(reference, other.reference);
		}

		public override bool Equals(object obj) => obj is WidgetContext other && Equals(other);
		public static bool operator ==(WidgetContext left, WidgetContext right) => left.Equals(right);
		public static bool operator !=(WidgetContext left, WidgetContext right) => !left.Equals(right);
	}
	
	public static class WidgetContextExtensions
	{
		public static WidgetContext AsContextId(this int _this, bool _isUnique=true)
		{
			return new WidgetContext(_this, null, _isUnique, default);
		}
		
		public static WidgetContext AsContextValue<TSource>(this TSource _this, bool _isUnique=true)
			where TSource:struct
		{
			return new WidgetContext(_this.GetHashCode(), typeof(TSource), _isUnique, default);
		}
		
		public static WidgetContext AsContextReference<TSource>(this TSource _this, bool _isUnique=true)
			where TSource:class
		{
			return new WidgetContext(_this.GetHashCode(), typeof(TSource), _isUnique, _this);
		}
	}
}
