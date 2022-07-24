using Speckle.ConnectorUnity.GUI;
using Speckle.ConnectorUnity.Ops;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SpeckleStreamObject))]
public class SpeckleStreamEditor : Editor
{
	SpeckleStreamObject obj;

	VisualElement root;

	VisualTreeAsset tree;

	void OnEnable()
	{
		tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GUIHelper.Dir + "SpeckleStream.uxml");
		obj = (SpeckleStreamObject)target;
	}

	public override VisualElement CreateInspectorGUI()
	{
		if (tree == null)
			return base.CreateInspectorGUI();

		root = new VisualElement();
		tree.CloneTree(root);

		var searchButton = root.Q<Button>("search-url");
		if (searchButton != null)
			searchButton.clickable.clicked += () =>
			{
				Debug.Log("Search button clicked");
				obj.Init();
			};

		return root;
	}
}