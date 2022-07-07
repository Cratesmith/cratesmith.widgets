using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.cratesmith.widgets
{
	public interface IWidgetPrefabDrawer
	{
	}
	
	[Serializable]
	public struct WidgetPrefab<TWidget> : IWidgetPrefabDrawer
		where TWidget : WidgetBehaviour
	{
		[FormerlySerializedAs("prefab")] 
		public WidgetBehaviour  root;
		public  TWidget         widget;
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
			var spPrefab = property.FindPropertyRelative("root");
			var spWidget = property.FindPropertyRelative("widget");
			var fieldType = spWidget.GetSerializedPropertyType();
			EditorGUI.PrefixLabel(position, label);
			
			var x = position.x + EditorGUIUtility.labelWidth;
			var width = (position.width-EditorGUIUtility.labelWidth)/2;
			var height = EditorGUI.GetPropertyHeight(property, true);

			var leftRect = new Rect(x, position.y, width, height);
			var rightRect = new Rect(x + width, position.y, width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(leftRect, spWidget, GUIContent.none);

			if (!spWidget.hasMultipleDifferentValues)
			{
				var widgetTransform = spWidget.objectReferenceValue
					? ((WidgetBehaviour)spWidget.objectReferenceValue).transform
					: null;
				
				var prefabTransform = spPrefab.objectReferenceValue
					? ((WidgetBehaviour)spPrefab.objectReferenceValue).transform
					: null;

				if (prefabTransform 
				    && (!widgetTransform || !widgetTransform.IsChildOf(prefabTransform)))
				{
					spPrefab.objectReferenceValue = null;
				}
			}
			
			if (spWidget.hasMultipleDifferentValues)
			{
				EditorGUI.PropertyField(rightRect, spWidget, GUIContent.none);
				return;
			}
			
			var widget = (WidgetBehaviour)spWidget.objectReferenceValue;
			if (Is.Null(widget))
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
						spWidget.objectReferenceValue = spPrefab.objectReferenceValue =  go.AddComponent(fieldType);
						EditorApplication.delayCall += () =>
						{
							if(go) go.SetActive(false);
						};
					}	
				}
				return;
			}
			
			var options = new List<Component>();
			options.Add(widget);
			options.AddRange(widget.GetComponentsInParent(typeof(WidgetBehaviour),true));

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

			var currentId = Math.Max(0,options.IndexOf((Component)spPrefab.objectReferenceValue));
			var newId = Mathf.Max(0, EditorGUI.Popup(rightRect, currentId, options.Select(x => x.name).ToArray()));
			if (newId != currentId)
			{
				spPrefab.objectReferenceValue = options[newId] != widget 
					? options[newId]
					: null;
			}
		}
	}
#endif
}
