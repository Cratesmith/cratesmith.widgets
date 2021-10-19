using UnityEngine;

namespace com.cratesmith.widgets
{
	public static class VectorExtensions
	{
		public static bool ApproxEquals(this Vector3 _a, in Vector3 _b)
		{
			return Mathf.Approximately(_a.x, _b.x)
			       && Mathf.Approximately(_a.y, _b.y);
		}

		public static bool ApproxEquals(this Vector2 _a, in Vector2 _b)
		{
			return Mathf.Approximately(_a.x, _b.x)
			       && Mathf.Approximately(_a.y, _b.y);
		}
	}
}
