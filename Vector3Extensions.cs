using UnityEngine;
namespace SubModules.com.cratesmith.widgets
{
	public static class Vector3Extensions
	{
		public static Vector2 XZ(this Vector3 @this)
		{
			return new Vector2(@this.x, @this.z);
		}

		public static Vector2 XY(this Vector3 @this)
		{
			return new Vector2(@this.x, @this.y);
		}	

		public static Vector3 X_Z(this Vector3 @this)
		{
			return new Vector3(@this.x, 0, @this.z);
		}
	
		public static float FlatDistSq(this Vector3 @from, Vector3 to)
		{
			var diff = (to-@from);
			diff.y = 0;
			return diff.sqrMagnitude;
		}
	
		public static float FlatDist(this Vector3 @this, Vector3 to)
		{
			return Mathf.Sqrt(FlatDistSq(@this, to));
		}
	}
}
