using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ScriptableStream))]
public class SpeckleStreamEditor : Editor
{
	ScriptableStream obj;
	VisualElement root;
	VisualTreeAsset tree;
	Button _searchButton;
	TextField _streamUrlField;

	void OnEnable()
	{
		obj = (ScriptableStream)target;
		obj.OnUpdate += SetStreamValues;

		tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GUIHelper.Folders.GUI + "SpeckleStream.uxml");
	}

	void OnDisable()
	{
		obj.OnUpdate -= SetStreamValues;
		_searchButton.clickable.clicked -= Search;
	}

	void SetStreamValues()
	{ }

	public override VisualElement CreateInspectorGUI()
	{
		if (tree == null)
			return base.CreateInspectorGUI();

		root = new VisualElement();
		tree.CloneTree(root);

		_searchButton = root.Q<Button>("search-button");

		if (_searchButton != null)
		{
			_searchButton.clickable.clicked += Search;
		}

		// All bounded to text fields 

		// _streamUrlField = root.Q<TextField>("stream-url");
		// _serverUrlField = root.Q<TextField>("server-url");
		// _streamIdField = root.Q<TextField>("stream-id");
		// _branchNameField = root.Q<TextField>("branch-name");
		// _commitIdField = root.Q<TextField>("commit-id");

		SetStreamValues();

		return root;
	}

	void Search()
	{
		Debug.Log("Starting Search");

		obj.Initialize(_streamUrlField.value).ContinueWith(() => { Debug.Log("Continue with call"); }).Forget();
	}

}