#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace com.cratesmith.widgets
{
	public static class WidgetHierarchyDrawer
	{
		[InitializeOnLoadMethod]
		static void InitializeOnLoad()
		{
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
		}
		static void HierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
		{
			var o = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
			if (o 
				&& (o.hideFlags&HideFlags.DontSave)!=0 
			    && o.gameObject.TryGetComponent(out WidgetBehaviour wb))
			{
				GUI.backgroundColor = new Color(.5f,.5f,.5f,.5f);
				GUI.Box(selectionrect, "");	
				GUI.backgroundColor = Color.white;
			}
		}
	}
}
#endif