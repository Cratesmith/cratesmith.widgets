using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace SubModules.com.cratesmith.widgets
{
	public static class RectUtil
	{
		public static Vector2 GetCorner(Rect rect, int index)
		{
			switch (index)
			{
				case 0: return new Vector2(rect.xMin, rect.yMin);  
				case 1: return new Vector2(rect.xMax, rect.yMin); 
				case 2: return new Vector2(rect.xMax, rect.yMax); 
				case 3: return new Vector2(rect.xMin, rect.yMax);
				default: 
					throw new IndexOutOfRangeException();
			}
		}
	}
}
