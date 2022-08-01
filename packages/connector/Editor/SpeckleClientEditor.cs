using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.GUI;
using Speckle.ConnectorUnity.Ops;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	public abstract class SpeckleClientEditor<TClient> : Editor where TClient : ClientBehaviour
	{
		protected TClient obj;

		protected DropdownField branches;

		protected DropdownField converters;

		protected ProgressBar progress;

		protected VisualElement root;

		protected Button runButton;

		protected Button searchButton;

		protected TextField streamUrlField;

		protected VisualTreeAsset tree;

		protected(string converterIndex, string branchIndex, string commitIndex, string nodeRoot) _fields;

		protected abstract string treePath { get; }

		protected int branchIndex
		{
			get => FindInt(_fields.branchIndex);
		}

		protected int converterIndex
		{
			get => FindInt(_fields.converterIndex);
		}

		protected int commitIndex
		{
			get => FindInt(_fields.commitIndex);
		}

		protected virtual void OnEnable()
		{
			tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treePath);

			obj = (TClient)target;
			obj.OnClientRefresh += RefreshAll;

			_fields.converterIndex = "_converterIndex";
			_fields.branchIndex = "_branchIndex";
			_fields.commitIndex = "_commitIndex";
			_fields.nodeRoot = "_root";
		}

		protected virtual void OnDisable()
		{
			obj.OnClientRefresh -= RefreshAll;
		}

		protected abstract void OnRunClicked();

		protected int FindInt(string propName) => serializedObject.FindProperty(propName).intValue;

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
			obj.SetBranch(index).Forget();
		}

		protected virtual void SetConverterChange(int index)
		{
			// obj.SetConverter(index);
		}

		public override VisualElement CreateInspectorGUI()
		{
			if (tree == null)
				return base.CreateInspectorGUI();

			root = new VisualElement();
			tree.CloneTree(root);

			root.Add(new PropertyField(serializedObject.FindProperty(_fields.nodeRoot)));

			branches = root.SetDropDown(
				"branch",
				branchIndex,
				obj.branches.Format(),
				e => branches.DropDownChange(e, SetBranchChange));

			// converters = root.SetDropDown(
			// 	"converter",
			// 	converterIndex,
			// 	obj.converters.Format(),
			// 	e => converters.DropDownChange(e, SetConverterChange));

			streamUrlField = root.Q<TextField>("url");
			streamUrlField.value = obj.GetUrl();

			searchButton = root.Q<Button>("search-button");
			// searchButton.clickable.clicked += () =>
			// {
			// 	if (SpeckleConnector.TryGetSpeckleStream(streamUrlField.value, out var speckleStream))
			// 		obj.SetStream(speckleStream);
			// };

			runButton = root.Q<Button>("run");
			runButton.clickable.clicked += OnRunClicked;

			progress = root.Q<ProgressBar>("progress");

			obj.OnTotalChildCountAction += value =>
			{
				progress.title = $"0/{value}";
				progress.highValue = value;
			};

			return root;
		}
	}
}