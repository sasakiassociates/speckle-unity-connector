using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Speckle.ConnectorUnity.GUI;
using Speckle.ConnectorUnity.Ops;
using UnityEditor.UIElements;

namespace Speckle.ConnectorUnity
{
	public abstract class SpeckleClientEditor<TClient> : Editor where TClient : SpeckleClient
	{

		protected DropdownField branches;
		protected DropdownField converters;
		protected TClient obj;
		protected ProgressBar progress;

		protected TextField streamUrlField;

		protected Button runButton;
		protected Button searchButton;

		protected VisualElement root;
		protected VisualTreeAsset tree;

		protected abstract string treePath { get; }

		protected int branchIndex => FindInt("branchIndex");
		protected int converterIndex => FindInt("converterIndex");

		protected virtual void OnEnable()
		{
			tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treePath);

			obj = (TClient)target;
			obj.onRepaint += RefreshAll;
		}

		protected virtual void OnDisable()
		{
			obj.onRepaint -= RefreshAll;
		}

		protected abstract void OnRunClicked();

		protected int FindInt(string propName)
		{
			return serializedObject.FindProperty(propName).intValue;
		}

		protected static void Refresh(DropdownField dropdown, IEnumerable<string> items, int index)
		{
			dropdown.choices = items.ToList();
			dropdown.index = index;
		}

		protected virtual void RefreshAll()
		{
			Refresh(branches, obj.branches.Format(), branchIndex);
		}

		protected virtual void SetBranchChange(int index)
		{
			obj.SetBranch(index);
		}

		protected virtual void SetConverterChange(int index)
		{
			obj.SetConverter(index);
		}

		public override VisualElement CreateInspectorGUI()
		{
			if (tree == null)
				return base.CreateInspectorGUI();

			root = new VisualElement();
			tree.CloneTree(root);
			
			root.Add(new PropertyField(serializedObject.FindProperty("_root")));

			branches = root.SetDropDown(
				"branch",
				FindInt("branchIndex"),
				obj.branches.Format(),
				e => branches.DropDownChange(e, SetBranchChange));

			converters = root.SetDropDown(
				"converter",
				FindInt("converterIndex"),
				obj.converters.Format(),
				e => converters.DropDownChange(e, SetConverterChange));

			streamUrlField = root.Q<TextField>("url");
			streamUrlField.value = obj.StreamUrl;

			searchButton = root.Q<Button>("search-button");
			searchButton.clickable.clicked += () =>
			{
				if (SpeckleConnector.TryGetSpeckleStream(streamUrlField.value, out var speckleStream))
					obj.SetStream(speckleStream);
			};

			runButton = root.Q<Button>("run");
			runButton.clickable.clicked += OnRunClicked;

			progress = root.Q<ProgressBar>("progress");

			obj.onTotalChildrenCountKnown += value =>
			{
				progress.title = $"0/{value}";
				progress.highValue = value;
			};

			obj.onProgressReport += values =>
			{
				// foreach (var v in values)
				// {
				// 	Debug.Log(v.Key + "-" + v.Value);
				// }
				// progress.value = values.Values.FirstOrDefault() / 100f;
			};

			return root;
		}
	}
}