using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
	public struct WidgetGridLayout : IEquatable<WidgetGridLayout>
	{
		public Optional<GridLayoutGroup.Corner>     startCorner;
		public Optional<GridLayoutGroup.Axis>       startAxis;
		public Optional<Vector2>                    cellSize;
		public Optional<Vector2>                    spacing;
		public Optional<GridLayoutGroup.Constraint> constraint;
		public Optional<int>                        constraintCount;
		public Optional<WidgetRectOffset>           padding;
		public Optional<TextAnchor>                 childAlignment;

		public WidgetGridLayout AsDefault()
		{
			return new WidgetGridLayout()
			{
				startCorner = this.startCorner.AsDefault(),
				startAxis = this.startAxis.AsDefault(),
				cellSize = this.cellSize.AsDefault(),
				spacing = this.spacing.AsDefault(),
				constraint = this.constraint.AsDefault(),
				constraintCount = this.constraintCount.AsDefault(),
				padding = this.padding.AsDefault(),
				childAlignment = this.childAlignment.AsDefault(),
			};
		}
        
		public bool Apply(GridLayoutGroup to, GridLayoutGroup defaults, bool usesTypePrefab)
		{
			bool changed = false;
			var _padding = padding.GetValue(defaults.padding, usesTypePrefab);
			var _startCorner = startCorner.GetValue(defaults.startCorner, usesTypePrefab);
			var _startAxis = startAxis.GetValue(defaults.startAxis, usesTypePrefab);
			var _cellSize = cellSize.GetValue(defaults.cellSize, usesTypePrefab);
			var _spacing = spacing.GetValue(defaults.spacing, usesTypePrefab);
			var _constraint = constraint.GetValue(defaults.constraint, usesTypePrefab);
			var _constraintCount = constraintCount.GetValue(defaults.constraintCount, usesTypePrefab);
			var _childAlignment = childAlignment.GetValue(defaults.childAlignment, usesTypePrefab);
			
			if(_padding != to.padding) { _padding.ApplyTo(to.padding); changed=true;}
			if(_startCorner != to.startCorner) { to.startCorner = _startCorner; changed=true;}
			if(_startAxis != to.startAxis) { to.startAxis = _startAxis; changed=true;}
			if(_cellSize != to.cellSize) { to.cellSize = _cellSize; changed=true;}
			if(_spacing != to.spacing) { to.spacing = _spacing; changed=true;}
			if(_constraint != to.constraint) { to.constraint = _constraint; changed=true;}
			if(_constraintCount != to.constraintCount) { to.constraintCount = _constraintCount; changed=true;}
			if(_childAlignment != to.childAlignment) { to.childAlignment = _childAlignment; changed=true;}
			return changed;
		}
		
		public override bool Equals(object obj)
		{
			return obj is WidgetGridLayout other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = startCorner.GetHashCode();
				hashCode = (hashCode * 397) ^ startAxis.GetHashCode();
				hashCode = (hashCode * 397) ^ cellSize.GetHashCode();
				hashCode = (hashCode * 397) ^ spacing.GetHashCode();
				hashCode = (hashCode * 397) ^ constraint.GetHashCode();
				hashCode = (hashCode * 397) ^ constraintCount.GetHashCode();
				hashCode = (hashCode * 397) ^ padding.GetHashCode();
				hashCode = (hashCode * 397) ^ childAlignment.GetHashCode();
				return hashCode;
			}
		}
		
		public bool Equals(WidgetGridLayout other)
		{
			return startCorner.Equals(other.startCorner) 
			       && startAxis.Equals(other.startAxis) 
			       && cellSize.Equals(other.cellSize) 
			       && spacing.Equals(other.spacing) 
			       && constraint.Equals(other.constraint) 
			       && constraintCount.Equals(other.constraintCount) 
			       && padding.Equals(other.padding) 
			       && childAlignment.Equals(other.childAlignment);
		}
	}
}
