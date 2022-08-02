using UnityEngine;

namespace com.cratesmith.widgets
{
	public struct EHovered : IWidgetEvent
	{
		public bool    hovered;
		public Vector2 position;
		public bool CheckValid() => hovered;
	}
}
