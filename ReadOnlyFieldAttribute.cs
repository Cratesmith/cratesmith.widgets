using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.cratesmith.widgets
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyFieldAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
        public class Drawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var prev = GUI.enabled; 
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = prev;
            }
        }
#endif
    }
}