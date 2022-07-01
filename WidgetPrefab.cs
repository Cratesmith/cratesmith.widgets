using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.cratesmith.widgets
{
	public interface IWidgetPrefabDrawer
	{
	}
	
	[Serializable]
	public struct WidgetPrefab<TWidget> : IWidgetPrefabDrawer
		where TWidget : WidgetBehaviour
	{
		public WidgetBehaviour prefab;
		public TWidget         widget;
	}
	
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(IWidgetPrefabDrawer), true)]
	public class WidgetPrefabReferenceDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var spPrefab = property.FindPropertyRelative("prefab");
			var spWidget = property.FindPropertyRelative("widget");
			var fieldType = spWidget.GetSerializedPropertyType();
			EditorGUI.PrefixLabel(position, label);
			
			var x = position.x + EditorGUIUtility.labelWidth;
			var width = (position.width-EditorGUIUtility.labelWidth)/2;
			var height = EditorGUI.GetPropertyHeight(property, true);

			var leftRect = new Rect(x, position.y, width, height);
			var rightRect = new Rect(x + width, position.y, width, height);
			EditorGUI.PropertyField(leftRect, spPrefab, GUIContent.none);
			if (spPrefab.hasMultipleDifferentValues)
			{
				EditorGUI.PropertyField(rightRect, spWidget, GUIContent.none);
				return;
			}
			
			var prefab = (WidgetBehaviour)spPrefab.objectReferenceValue;
			if (!Is.NotNull(prefab))
			{
				var serializedObject = property.serializedObject;
				if (!serializedObject.isEditingMultipleObjects && serializedObject.targetObject is WidgetBehaviour widgetBehaviour && widgetBehaviour.gameObject.scene.IsValid())
				{
					if (GUI.Button(rightRect,"Create"))
					{
						var newObjName = property.name.StartsWith("m_") 
							? $"_{property.name.Substring(2, property.name.Length-2)}"
							: $"_{property.name}";
						var go = new GameObject(newObjName);
						go.transform.SetParent(widgetBehaviour.transform);
						spPrefab.objectReferenceValue =  go.AddComponent(fieldType);
						EditorApplication.delayCall += () =>
						{
							if(go) go.SetActive(false);
						};
					}	
				}
				
				return;
			}
			
			var options = new List<Component>();
			options.AddRange(prefab.GetComponentsInChildren(fieldType));
			if (prefab.TryGetComponent(fieldType, out var rootComp) && !options.Contains(rootComp))
				options.Insert(0, rootComp);

			if (options.Count == 0)
			{
				using (new EditorGUI.DisabledScope(true))
				{
					GUI.backgroundColor = Color.red;
					EditorGUI.Popup(rightRect, 0, new[] { fieldType.Name });
					GUI.backgroundColor = Color.white;
				}
				spWidget.objectReferenceValue = null;
				return;
			}

			var currentId = options.IndexOf((Component)spWidget.objectReferenceValue);
			var newId = Mathf.Max(0, EditorGUI.Popup(rightRect, currentId, options.Select(x => x.name).ToArray()));
			if (newId != currentId)
			{
				spWidget.objectReferenceValue = options[newId];
			}
		}
	}
#endif
}
