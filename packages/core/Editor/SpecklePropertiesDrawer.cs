using Speckle.ConnectorUnity.Models;
using UnityEditor;
using UnityEngine;

namespace Speckle.ConnectorUnity
{

	[CustomPropertyDrawer(typeof(SpeckleProperties))]
	public class SpecklePropertiesDrawer : PropertyDrawer
	{

		public bool _show = true;

		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			// position = EditorGUILayout.BeginHorizontal();

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Properties"));

			var indent = EditorGUI.indentLevel;

			EditorGUI.indentLevel = 0;

			
			_show = EditorGUILayout.Foldout(_show, GUIContent.none);
			// _show = EditorGUI.Foldout(position, _show, GUIContent.none);
			// EditorGUILayout.EndHorizontal();

			if (_show)
			{
				EditorGUI.indentLevel++;

				var prop = (SpeckleProperties)EditorHelper.GetTargetObjectOfProperty(property);
				EditorGUILayout.TextField("0");
				EditorGUILayout.TextField("1");

				foreach (var VARIABLE in prop.Data)
				{ }
			}

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}