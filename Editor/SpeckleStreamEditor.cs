using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.GUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SpeckleStream))]
public class SpeckleStreamEditor : Editor
{
	private SpeckleStream obj;
	private VisualElement root;
	private VisualTreeAsset tree;

	private void OnEnable()
	{
		tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GUIHelper.Dir + "SpeckleStream.uxml");
		obj = (SpeckleStream)target;
	}

	public override VisualElement CreateInspectorGUI()
	{
		if (tree == null)
			return base.CreateInspectorGUI();

		root = new VisualElement();
		tree.CloneTree(root);

		var searchButton = root.Q<Button>("search-url");
		if (searchButton != null)
		{
			searchButton.clickable.clicked += () =>
			{
				Debug.Log("Search button clicked");
				obj.Init();
			};
		}

		return root;
	}
}