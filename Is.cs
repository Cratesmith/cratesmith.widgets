using UnityEngine;

namespace com.cratesmith.widgets
{
	public static class Is
	{
		public static bool Spawned(WidgetBehaviour widget)
		{
			return (bool)(MonoBehaviour)widget && !widget.Despawned;
		}

		public static bool NotNull<T>(T value)
		{
			if (!typeof(T).IsClass)
			{
				return true;
			}

			if (value is Object obj)
			{
				return obj;
			}

			return !ReferenceEquals(value, null);
		}

		public static bool Null<T>(T value)
		{
			if (!typeof(T).IsClass)
			{
				return false;
			}

			if (value is Object obj)
			{
				return !obj;
			}

			return ReferenceEquals(value, null);
		}
	}
}
