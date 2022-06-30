using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.GUI;
using Speckle.ConnectorUnity.Ops;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	[CustomEditor(typeof(Receiver))]
	public class ReceiverEditor : SpeckleClientEditor<Receiver>
	{

		DropdownField commits;

		StreamPreview preview;

		Toggle showPreview, renderPreview;
		
		protected override string treePath
		{
			get => GUIHelper.Dir + "Receiver.uxml";
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			obj.OnPreviewSet += SetPreview;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			obj.OnPreviewSet -= SetPreview;
		}

		public override VisualElement CreateInspectorGUI()
		{
			root = base.CreateInspectorGUI();

			if (tree == null)
				return root;

			commits = root.SetDropDown(
				"commit",
				commitIndex,
				obj.Commits.Format(),
				e => commits.DropDownChange(e, i => { obj.SetCommit(i); }));

			preview = root.Q<StreamPreview>("preview");
			preview.thumbnail.image = GetPreview();

			showPreview = root.Q<Toggle>("show-preview");
			showPreview.RegisterCallback<ClickEvent>(_ => { preview.thumbnail.image = GetPreview(); });

			renderPreview = root.Q<Toggle>("render-preview");
			renderPreview.RegisterCallback<ClickEvent>(_ => obj.RenderPreview());

			return root;
		}

		protected override void OnRunClicked()
		{
			if (!obj.isWorking)
				obj.Receive().Forget();
		}

		protected override void SetBranchChange(int index)
		{
			base.SetBranchChange(index);
			Refresh(commits, obj.Commits.Format(), commitIndex);
		}

		Texture GetPreview() => obj.ShowPreview ? obj.Preview : null;

		void SetPreview()
		{
			preview.thumbnail.image = obj.Preview;
		}

		protected override void RefreshAll()
		{
			base.RefreshAll();

			Refresh(commits, obj.Commits.Format().ToList(), commitIndex);
		}
	}

}