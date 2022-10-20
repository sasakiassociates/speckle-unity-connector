using UnityEditor;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	[CustomPropertyDrawer( typeof( ReadOnlyAttribute ) )]
	public class ReadOnlyDrawer : PropertyDrawer {

		public override void OnGUI( Rect position,
		                            SerializedProperty property,
		                            GUIContent label )
		{
			UnityEngine.GUI.enabled = false;
			EditorGUI.PropertyField( position, property, label, true );
			UnityEngine.GUI.enabled = true;
		}

	}

}