using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.GUI;
using Speckle.ConnectorUnity.Ops;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	[CustomEditor(typeof(Receiver))]
	public class ReceiverEditor : SpeckleClientEditor<Receiver,ReceiveWorkArgs>
	{

		DropdownField commits;

		StreamPreview preview;

		Toggle showPreview, renderPreview;

		protected override string treePath
		{
			get => GUIHelper.Folders.GUI + "Receiver.uxml";
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
			// renderPreview.RegisterCallback<ClickEvent>(_ => obj.Re());

			return root;
		}

		protected override void OnRunClicked()
		{
			if (!obj.IsWorking)
				obj.DoWork().Forget();
		}

		protected override void SetBranchChange(int index)
		{
			base.SetBranchChange(index);
			Refresh(commits, obj.Commits.Format(), commitIndex);
		}

		Texture GetPreview() => obj.showPreview ? obj.preview : null;

		void SetPreview()
		{
			preview.thumbnail.image = obj.preview;
		}

		protected override void RefreshAll()
		{
			base.RefreshAll();

			Refresh(commits, obj.Commits.Format().ToList(), commitIndex);
		}
	}

}