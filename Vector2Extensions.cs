using UnityEngine;
namespace SubModules.com.cratesmith.widgets
{
	public static class Vector2Extensions
	{
		public static Vector3 X_Y(this Vector2 @this)
		{
			return new Vector3(@this.x, 0, @this.y);
		}

		public static Vector3 XY_(this Vector2 @this)
		{
			return new Vector3(@this.x, @this.y, 0f);
		}

		public static float ToAngle(this Vector2 @this)
		{
			return 90-Mathf.Atan2(@this.y, @this.x) * Mathf.Rad2Deg;
		}
	}
}
