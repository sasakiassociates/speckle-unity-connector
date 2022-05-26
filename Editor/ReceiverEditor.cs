using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.GUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	[CustomEditor(typeof(Receiver))]
	public class ReceiverEditor : SpeckleClientEditor<Receiver>
	{

		private DropdownField commits;

		private StreamPreview preview;

		private Button searchButton;
		private Toggle showPreview, renderPreview;
		private TextField streamUrlField;

		private int commitIndex => FindInt("commitIndex");

		protected override string treePath => GUIHelper.Dir + "Receiver.uxml";

		protected override void OnEnable()
		{
			base.OnEnable();

			obj.onPreviewSet += SetPreview;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			obj.onPreviewSet -= SetPreview;
		}

		public override VisualElement CreateInspectorGUI()
		{
			root = base.CreateInspectorGUI();

			if (tree == null)
				return root;

			commits = root.SetDropDown(
				"commit",
				FindInt("commitIndex"),
				obj.Commits.Format(),
				e => commits.DropDownChange(e, i => { obj.SetCommit(i); }));

			streamUrlField = root.Q<TextField>("url");
			streamUrlField.value = obj.StreamUrl;

			searchButton = root.Q<Button>("search-button");
			searchButton.clickable.clicked += () =>
			{
				if (SpeckleConnector.TryGetSpeckleStream(streamUrlField.value, out var speckleStream))
					obj.SetStream(speckleStream);
			};

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

		private Texture GetPreview()
		{
			return obj.ShowPreview ? obj.Preview : null;
		}

		private void SetPreview()
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