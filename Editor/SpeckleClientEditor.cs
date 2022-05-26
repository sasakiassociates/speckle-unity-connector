using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Speckle.ConnectorUnity.GUI;

namespace Speckle.ConnectorUnity
{
	public abstract class SpeckleClientEditor<TClient> : Editor where TClient : SpeckleClient
	{

		protected DropdownField branches;
		protected DropdownField converters;
		protected TClient obj;
		protected ProgressBar progress;

		protected VisualElement root;

		protected Button runButton;
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