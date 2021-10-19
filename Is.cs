using UnityEngine;

namespace com.cratesmith.widgets
{
	public static class Is
	{
		public static bool Spawned(this WidgetBehaviour widget)
		{
			return (bool)((MonoBehaviour)widget) && !widget.Despawned;
		}
        
		public static bool NotNull<T>(this T value) where T:class
		{
			if (value is UnityEngine.Object obj) 
				return obj;
            
			return !ReferenceEquals(value, null);
		}
        
		public static bool Null<T>(this T value) where T:class
		{
			if (value is UnityEngine.Object obj) 
				return !obj;
            
			return ReferenceEquals(value, null);
		}
	}
}
