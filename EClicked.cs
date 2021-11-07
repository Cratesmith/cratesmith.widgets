using UnityEngine;

namespace com.cratesmith.widgets
{
	public struct EClicked : IWidgetEvent
	{
		public int frame;
		public bool CheckValid() => frame == Time.frameCount;
		public static EClicked ThisFrame() => new EClicked()
		{
			frame = Time.frameCount,
		};
	}
}
