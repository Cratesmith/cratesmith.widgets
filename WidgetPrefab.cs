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
		public WidgetBehaviour prefab;
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
			EditorGUI.PrefixLabel(position, label);

			var x = position.x + EditorGUIUtility.labelWidth;
			var height = EditorGUIUtility.singleLineHeight;

			var buttonRect = new Rect(x, position.y, height, height);
			property.isExpanded = GUI.Toggle(buttonRect, property.isExpanded, new GUIContent(""), "button"); 
			var imageRect = new Rect(buttonRect.x+1, buttonRect.y+1, buttonRect.width-2, buttonRect.height-2);
			GUI.DrawTexture(imageRect, EditorGUIUtility.ObjectContent(null, property.isExpanded 
				                                                          ? typeof(SceneAsset)
				                                                          : typeof(GameObject)).image);
			
			if(property.isExpanded)
				OnGUI_FromScene(position, property);
			else 
				OnGUI_FromPrefab(position, property);
		}
		
		void OnGUI_FromPrefab(Rect position, SerializedProperty property)
		{
			var spPrefab = property.FindPropertyRelative("prefab");
			var spWidget = property.FindPropertyRelative("widget");
			var fieldType = spWidget.GetSerializedPropertyType(); 
			
			var height = EditorGUIUtility.singleLineHeight;
			var x = position.x + EditorGUIUtility.labelWidth + height;
			var width = (position.width-EditorGUIUtility.labelWidth-height);
			var leftRect = new Rect(x, position.y, width*.6f, height);
			var rightRect = new Rect(x + width*.6f, position.y, width*.4f, EditorGUIUtility.singleLineHeight);

			EditorGUI.PropertyField(leftRect, spPrefab, GUIContent.none);
			
			if (spWidget.hasMultipleDifferentValues)
			{
				EditorGUI.PropertyField(rightRect, spWidget, GUIContent.none);
				return;
			}
			
			var widget = (WidgetBehaviour)spWidget.objectReferenceValue;
			var prefab = spPrefab.objectReferenceValue 
				? (WidgetBehaviour)spPrefab.objectReferenceValue
				: widget;
			
			if (Is.Null(prefab))
			{
				var serializedObject = property.serializedObject;
				if (!serializedObject.isEditingMultipleObjects && serializedObject.targetObject is WidgetBehaviour widgetBehaviour && widgetBehaviour.gameObject.scene.IsValid())
				{
					if (GUI.Button(rightRect, "Create"))
					{
						var newObjName = property.name.StartsWith("m_")
							? $"_{property.name.Substring(2, property.name.Length - 2)}"
							: $"_{property.name}";
						var go = new GameObject(newObjName);
						go.transform.SetParent(widgetBehaviour.transform);
						spWidget.objectReferenceValue = spPrefab.objectReferenceValue = go.AddComponent(fieldType);
						EditorApplication.delayCall += () =>
						{
							if (go) go.SetActive(false);
						};
					}
				}
				return;
			}

			var options = new List<Component>();
			if(Is.NotNull(prefab))
				options.AddRange(prefab.GetComponentsInChildren(fieldType, true));

			if (options.Count == 0)
			{
				EditorGUI.HelpBox(rightRect, $"No {fieldType.Name}!", MessageType.Warning);
				spWidget.objectReferenceValue = null;
				return;
			}

			var currentId = options.IndexOf((Component)spWidget.objectReferenceValue);
			var newId =  EditorGUI.Popup(rightRect, currentId, options.Select(x => x.name).ToArray());
			if(newId!=currentId)
				spWidget.objectReferenceValue = newId >= 0 ? options[newId]:null;
		}
		
		static void OnGUI_FromScene(Rect position, SerializedProperty property)
		{
			var spPrefab = property.FindPropertyRelative("prefab");
			var spWidget = property.FindPropertyRelative("widget");
			var fieldType = spWidget.GetSerializedPropertyType(); 
			
			var height = EditorGUIUtility.singleLineHeight;
			var x = position.x + EditorGUIUtility.labelWidth + height;
			var width = (position.width-EditorGUIUtility.labelWidth-height);
			var leftRect = new Rect(x, position.y, width*.4f, height);
			var rightRect = new Rect(x + width*.4f, position.y, width*.6f, EditorGUIUtility.singleLineHeight);

			EditorGUI.PropertyField(rightRect, spWidget, GUIContent.none);

			if (!spWidget.hasMultipleDifferentValues)
			{
				var widgetTransform = spWidget.objectReferenceValue
					? ((WidgetBehaviour)spWidget.objectReferenceValue).transform
					: null;

				var prefabTransform = spPrefab.objectReferenceValue
					? ((WidgetBehaviour)spPrefab.objectReferenceValue).transform
					: null;

				if (prefabTransform 
				    && prefabTransform!=widgetTransform
				    && widgetTransform && !widgetTransform.IsChildOf(prefabTransform))
				{
					spPrefab.objectReferenceValue = spWidget.objectReferenceValue;
				}
			}

			if (spWidget.hasMultipleDifferentValues)
			{
				EditorGUI.PropertyField(leftRect, spWidget, GUIContent.none);
				return;
			}

			var widget = (WidgetBehaviour)spWidget.objectReferenceValue;
			if (Is.Null(widget))
			{
				var serializedObject = property.serializedObject;
				if (!serializedObject.isEditingMultipleObjects && serializedObject.targetObject is WidgetBehaviour widgetBehaviour && widgetBehaviour.gameObject.scene.IsValid())
				{
					if (GUI.Button(leftRect, "Create"))
					{
						var newObjName = property.name.StartsWith("m_")
							? $"_{property.name.Substring(2, property.name.Length - 2)}"
							: $"_{property.name}";
						var go = new GameObject(newObjName);
						go.transform.SetParent(widgetBehaviour.transform);
						spWidget.objectReferenceValue = spPrefab.objectReferenceValue = go.AddComponent(fieldType);
						EditorApplication.delayCall += () =>
						{
							if (go) go.SetActive(false);
						};
					}
				}
				return;
			}

			var options = new List<Component>();
			
			options.AddRange(widget.GetComponentsInParent(typeof(WidgetBehaviour), true));

			var currentId = options.IndexOf((Component)spPrefab.objectReferenceValue);
			var newId = EditorGUI.Popup(leftRect, currentId, options.Select(x => x.name).ToArray());
			if(currentId!=newId)
				spPrefab.objectReferenceValue = newId >= 0 ? options[newId]:null;
		}
	}
#endif
}
